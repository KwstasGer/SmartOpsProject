using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartOps.Models;
using SmartOps.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;


namespace SmartOps.Controllers
{
    public class CustomersController : Controller
    {
        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;
        private readonly CustomerService _customerService;

        public CustomersController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        // ----------------------- INDEX -----------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var customers = await _customerService.GetAllByUserAsync(CurrentUserId);
            return View(customers);
        }

        // ----------------------- CREATE (GET) -----------------------
        [HttpGet]
        public IActionResult Create()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            var model = new Customer
            {
                UserId = CurrentUserId,
                CustomerCode = "*" // προεπιλογή: '*' σημαίνει αυτόματος κωδικός
            };

            SetDropDowns();
            return View(model);
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,TaxIdentificationNumber,Country,Address,City,PostalCode,VatStatus,CustomerCategory,CustomerCode")]
    Customer customer)
        {
         
            // Έλεγχος αν ο χρήστης είναι συνδεδεμένος
            var uid = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (uid == 0)
            {
                TempData["ErrorMessage"] = "Η συνεδρία έληξε. Παρακαλώ συνδεθείτε ξανά.";
                return RedirectToAction("Login", "Account");
            }


            ModelState.Remove(nameof(Customer.User));

            // Σύνδεση του πελάτη με τον χρήστη που τον δημιούργησε
            customer.UserId = uid;
            
            // Έλεγχος για manual ή αυτόματο CustomerCode
            var codeInput = customer.CustomerCode?.Trim();

            // Αν ο χρήστης αφήσει κενό, "*" ή "-" → γίνεται αυτόματη αρίθμηση
            var autoCode = string.IsNullOrEmpty(codeInput) || codeInput == "*" || codeInput == "-";

            if (autoCode)
            {
                // Παραγωγή του επόμενου διαθέσιμου κωδικού
                customer.CustomerCode = await GenerateNextCustomerCodeAsync(uid);
            }        
            // Re-validation με τον πραγματικό CustomerCode            
            ModelState.Clear();
            TryValidateModel(customer);

            if (!ModelState.IsValid)
            {
                // Φόρτωση dropdowns (VAT Status & Category) για να εμφανιστούν σωστά στο View
                SetDropDowns(customer.VatStatus, customer.CustomerCategory);

                // Αν ο χρήστης είχε ζητήσει αυτόματη αρίθμηση, ξαναδείξε "*"
                if (autoCode)
                    customer.CustomerCode = "*";

                return View(customer);
            }
            // Αποθήκευση στη βάση δεδομένων
            try
            {
                await _customerService.AddAsync(customer);

                TempData["SuccessMessage"] = "Ο πελάτης δημιουργήθηκε με επιτυχία.";
                return RedirectToAction(nameof(Index));
            }           
            // Unique violation (διπλό CustomerCode)
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql &&
                                               (sql.Number == 2601 || sql.Number == 2627))
            {
                if (autoCode)
                {
                    // Σφάλμα κατά την αυτόματη δημιουργία κωδικού
                    ModelState.AddModelError(string.Empty, "Παρουσιάστηκε σφάλμα στην αυτόματη δημιουργία κωδικού.");
                }
                else
                {
                    // Ο κωδικός υπάρχει ήδη
                    ModelState.AddModelError(nameof(Customer.CustomerCode), "Ο κωδικός χρησιμοποιείται ήδη.");
                }
            }
            // Γενικά DB errors
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "Σφάλμα βάσης δεδομένων: " +
                                                       (ex.InnerException?.Message ?? ex.Message));
            }
            // Επιστροφή στη φόρμα αν κάτι απέτυχε
            SetDropDowns(customer.VatStatus, customer.CustomerCategory);

            if (autoCode)
                customer.CustomerCode = "*";   // UX: εμφανίζεται όπως το έδωσε ο χρήστης

            return View(customer);
        }





        // ----------------------- EDIT (GET) -----------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0) return BadRequest();
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            var customer = await _customerService.GetByIdForUserAsync(id, CurrentUserId);
            if (customer == null) return NotFound();

            SetDropDowns(customer.VatStatus, customer.CustomerCategory);
            return View(customer);
        }

        // ----------------------- EDIT (POST) -----------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            // Δεν επιτρέπουμε αλλαγή CustomerCode στο Edit
            [Bind("Id,Name,TaxIdentificationNumber,Country,Address,City,PostalCode,VatStatus,CustomerCategory")]
    Customer form)
        {
            var uid = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (uid == 0)
            {
                TempData["ErrorMessage"] = "Η συνεδρία έληξε. Παρακαλώ συνδεθείτε ξανά.";
                return RedirectToAction("Login", "Account");
            }

            if (id != form.Id) return BadRequest();

            var belongsToUser = await _customerService.ExistsForUserAsync(id, uid);
            if (!belongsToUser) return NotFound();

            // 🔑 ΜΗΝ απαιτείς CustomerCode στο Edit
            ModelState.Remove(nameof(Customer.CustomerCode));
            ModelState.Remove(nameof(Customer.User)); // navigation

            if (!ModelState.IsValid)
            {
                SetDropDowns(form.VatStatus, form.CustomerCategory);
                return View(form);
            }

            // Φέρνουμε το υπάρχον record για να ΜΗΝ αλλάξουμε το CustomerCode
            var existing = await _customerService.GetByIdForUserAsync(id, uid);
            if (existing == null) return NotFound();

            existing.Name = form.Name;
            existing.TaxIdentificationNumber = form.TaxIdentificationNumber;
            existing.Country = form.Country;
            existing.Address = form.Address;
            existing.City = form.City;
            existing.PostalCode = form.PostalCode;
            existing.VatStatus = form.VatStatus;
            existing.CustomerCategory = form.CustomerCategory;
            existing.UserId = uid;

            await _customerService.UpdateAsync(existing);
            TempData["SuccessMessage"] = "Ο πελάτης ενημερώθηκε με επιτυχία.";
            return RedirectToAction(nameof(Index));
        }


        // ----------------------- DELETE (GET) -----------------------
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest();
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            var customer = await _customerService.GetByIdForUserAsync(id, CurrentUserId);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // ----------------------- DELETE (POST) -----------------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            var customer = await _customerService.GetByIdForUserAsync(id, CurrentUserId);
            if (customer == null) return NotFound();

            try
            {
                await _customerService.DeleteAsync(customer);
                TempData["SuccessMessage"] = "Ο πελάτης διαγράφηκε με επιτυχία.";
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && sql.Number == 547) // FK
            {
                TempData["ErrorMessage"] = "Ο πελάτης δεν μπορεί να διαγραφεί γιατί έχει συνδεδεμένα παραστατικά.";
            }

            return RedirectToAction(nameof(Index));
        }


        // ----------------------- DETAILS -----------------------
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            var customer = await _customerService.GetByIdForUserAsync(id, CurrentUserId);
            return customer == null ? NotFound() : View(customer);
        }

        // ----------------------- HELPERS -----------------------
        private void SetDropDowns(string? selectedVat = null, string? selectedCategory = null)
        {
            ViewBag.VatStatuses = new SelectList(
                new List<string> { "Κανονικό", "Μειωμένο", "Απαλλασσόμενο" },
                selectedVat
            );

            ViewBag.CustomerCategories = new SelectList(
                new List<string> { "Λιανικής", "Χονδρικής" },
                selectedCategory
            );


        }
        // Υπολογισμός επόμενου κωδικού πελάτη (0001, 0002, ...)
        private async Task<string> GenerateNextCustomerCodeAsync(int userId)
        {
            var allForUser = await _customerService.GetAllByUserAsync(userId);

            var numeric = allForUser
                .Where(c => !string.IsNullOrWhiteSpace(c.CustomerCode) &&
                            c.CustomerCode.All(char.IsDigit))
                .Select(c => int.TryParse(c.CustomerCode, out var n) ? n : 0);

            var next = (numeric.Any() ? numeric.Max() : 0) + 1;
            return next.ToString("D4"); // 0001, 0002, 0003...
        }

    }
}

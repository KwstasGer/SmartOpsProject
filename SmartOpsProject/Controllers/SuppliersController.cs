using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SmartOpsProject.Models;
using SmartOpsProject.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SmartOps.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly SupplierService _supplierService;
        public SuppliersController(SupplierService supplierService) => _supplierService = supplierService;

        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

        // ----------------------- INDEX -----------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var suppliers = await _supplierService.GetAllByUserAsync(CurrentUserId);
            return View(suppliers);
        }

        // ----------------------- CREATE (GET) ----------------
        [HttpGet]
        public IActionResult Create()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            SetDropDowns();
            var model = new Supplier
            {
                UserId = CurrentUserId,
                SupplierCode = "*" // όπως στους πελάτες
            };
            return View(model);
        }

        // ----------------------- CREATE (POST) ---------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            var uid = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (uid == 0) return RedirectToAction("Login", "Account");

            ModelState.Remove(nameof(Supplier.User)); // navigation
            supplier.UserId = uid;

            // * / - / κενό => αυτόματος κωδικός
            var code = supplier.SupplierCode?.Trim();
            if (string.IsNullOrEmpty(code) || code == "*" || code == "-")
                supplier.SupplierCode = await GenerateNextSupplierCodeAsync(uid);

            // Επανα-validate
            ModelState.Clear();
            TryValidateModel(supplier);

            if (!ModelState.IsValid)
            {
                SetDropDowns(supplier.SupplierCategory, supplier.VatStatus);
                if (string.IsNullOrWhiteSpace(code) || code == "*") supplier.SupplierCode = "*";
                return View(supplier);
            }

            try
            {
                await _supplierService.AddAsync(supplier);
                TempData["SuccessMessage"] = "Ο προμηθευτής δημιουργήθηκε.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql &&
                                               (sql.Number == 2601 || sql.Number == 2627))
            {
                ModelState.AddModelError(nameof(Supplier.SupplierCode), "Ο κωδικός χρησιμοποιείται ήδη.");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "DB error: " + (ex.InnerException?.Message ?? ex.Message));
            }

            SetDropDowns(supplier.SupplierCategory, supplier.VatStatus);
            return View(supplier);
        }

        // ----------------------- EDIT (GET) ------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var supplier = await _supplierService.GetByIdForUserAsync(id, CurrentUserId);
            if (supplier == null) return NotFound();
            SetDropDowns(supplier.SupplierCategory, supplier.VatStatus);
            return View(supplier);
        }

        // ----------------------- EDIT (POST) -----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            // ❌ δεν επιτρέπουμε αλλαγή κωδικού στο Edit
            [Bind("Id,Name,TaxIdentificationNumber,Country,Address,City,PostalCode,SupplierCategory,VatStatus")]
            Supplier input)
        {
            var uid = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (uid == 0) return RedirectToAction("Login", "Account");
            if (id != input.Id) return BadRequest();

            var supplier = await _supplierService.GetByIdForUserAsync(id, uid);
            if (supplier == null) return NotFound();

            // Μην απαιτείς/μην αλλάζεις SupplierCode
            ModelState.Remove(nameof(Supplier.SupplierCode));
            ModelState.Remove(nameof(Supplier.User));

            if (!ModelState.IsValid)
            {
                SetDropDowns(input.SupplierCategory, input.VatStatus);
                return View(input);
            }

            supplier.UserId = uid;
            supplier.Name = input.Name;
            supplier.TaxIdentificationNumber = input.TaxIdentificationNumber;
            supplier.Country = input.Country;
            supplier.Address = input.Address;
            supplier.City = input.City;
            supplier.PostalCode = input.PostalCode;
            supplier.SupplierCategory = input.SupplierCategory;
            supplier.VatStatus = input.VatStatus;

            await _supplierService.UpdateAsync(supplier);
            TempData["SuccessMessage"] = "Ο προμηθευτής ενημερώθηκε.";
            return RedirectToAction(nameof(Index));
        }

        // ----------------------- DETAILS ---------------------
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var supplier = await _supplierService.GetByIdForUserAsync(id, CurrentUserId);
            return supplier == null ? NotFound() : View(supplier);
        }

        // ----------------------- DELETE (GET) ----------------
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var supplier = await _supplierService.GetByIdForUserAsync(id, CurrentUserId);
            if (supplier == null) return NotFound();
            return View(supplier);
        }

        // ----------------------- DELETE (POST) ---------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var supplier = await _supplierService.GetByIdForUserAsync(id, CurrentUserId);
            if (supplier == null) return NotFound();

            await _supplierService.DeleteAsync(supplier);
            TempData["SuccessMessage"] = "Ο προμηθευτής διαγράφηκε.";
            return RedirectToAction(nameof(Index));
        }

        // ----------------------- Helpers ---------------------
        private void SetDropDowns(string? category = null, string? vat = null)
        {
            ViewBag.SupplierCategories = new SelectList(
                new System.Collections.Generic.List<string> { "Εσωτερικού", "ΕΕ", "Τρίτων Χωρών" }, category);
            ViewBag.VatStatuses = new SelectList(
                new System.Collections.Generic.List<string> { "Κανονικό", "Μειωμένο", "Απαλλάσσεται" }, vat);
        }

        // Υπολογισμός επόμενου κωδικού (0001, 0002, ...)
        private async Task<string> GenerateNextSupplierCodeAsync(int userId)
        {
            var allForUser = await _supplierService.GetAllByUserAsync(userId);

            var numeric = allForUser
                .Where(s => !string.IsNullOrWhiteSpace(s.SupplierCode) && s.SupplierCode.All(char.IsDigit))
                .Select(s => int.TryParse(s.SupplierCode, out var n) ? n : 0);

            var next = (numeric.Any() ? numeric.Max() : 0) + 1;
            return next.ToString("D4");
        }
    }
}

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartOpsProject.Models;
using SmartOpsProject.Services;

namespace SmartOps.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ServiceService _serviceService;

        public ServicesController(ServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

        // ----------------------- INDEX -----------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var services = await _serviceService.GetAllByUserAsync(CurrentUserId);
            return View(services);
        }

        // ----------------------- CREATE (GET) ----------------
        [HttpGet]
        public IActionResult Create()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            SetUnits();          // dropdown Μονάδων
            SetVATs(null);       // dropdown ΦΠΑ (0,6,13,24)

            var model = new Service
            {
                UserId = CurrentUserId,
                ServiceCode = "*"   // * = αυτόματος κωδικός, όπως στους πελάτες/προμηθευτές
            };

            return View(model);
        }

        // ----------------------- CREATE (POST) --------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            var uid = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (uid == 0) return RedirectToAction("Login", "Account");

            // να μη γίνεται validate το navigation
            ModelState.Remove(nameof(Service.User));

            service.UserId = uid;

            // Τι έγραψε ο χρήστης στο πεδίο κωδικού;
            var codeInput = service.ServiceCode?.Trim();
            var autoCode = string.IsNullOrEmpty(codeInput) || codeInput == "*" || codeInput == "-";

            // ⭐ Αν είναι *, -, κενό => αυτόματη αρίθμηση (0001, 0002, ...)
            if (autoCode)
            {
                service.ServiceCode = await GenerateNextServiceCodeAsync(uid);
            }

            // Re-validate με τον τελικό κωδικό
            ModelState.Clear();
            TryValidateModel(service);

            if (!ModelState.IsValid)
            {
                SetUnits(service.Unit);
                SetVATs(service.VAT);

                // Να ξαναφαίνεται * στο UI αν είχε ζητήσει auto
                if (autoCode) service.ServiceCode = "*";

                return View(service);
            }

            try
            {
                await _serviceService.AddAsync(service);
                TempData["SuccessMessage"] = "Η υπηρεσία δημιουργήθηκε με επιτυχία.";
                return RedirectToAction(nameof(Index));
            }
            // Unique violation στο ServiceCode (αν έχεις unique index)
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql &&
                                               (sql.Number == 2601 || sql.Number == 2627))
            {
                if (autoCode)
                    ModelState.AddModelError(string.Empty, "Παρουσιάστηκε σφάλμα στην αυτόματη δημιουργία κωδικού.");
                else
                    ModelState.AddModelError(nameof(Service.ServiceCode), "Ο κωδικός χρησιμοποιείται ήδη.");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "DB error: " + (ex.InnerException?.Message ?? ex.Message));
            }

            SetUnits(service.Unit);
            SetVATs(service.VAT);
            if (autoCode) service.ServiceCode = "*";
            return View(service);
        }

        // ----------------------- EDIT (GET) ------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            var service = await _serviceService.GetByIdForUserAsync(id, CurrentUserId);
            if (service == null) return NotFound();

            SetUnits(service.Unit);
            SetVATs(service.VAT);

            return View(service);
        }

        // ----------------------- EDIT (POST) -----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service input)
        {
            var uid = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (uid == 0) return RedirectToAction("Login", "Account");
            if (id != input.Id) return BadRequest();

            var service = await _serviceService.GetByIdForUserAsync(id, uid);
            if (service == null) return NotFound();

            ModelState.Remove(nameof(Service.User));

            if (!ModelState.IsValid)
            {
                SetUnits(input.Unit);
                SetVATs(input.VAT);
                return View(input);
            }

            // Ενημέρωση επιτρεπτών πεδίων
            service.UserId = uid;
            service.ServiceCode = input.ServiceCode;   // στο Edit είναι readonly+hidden, άρα μένει ίδιο
            service.Description = input.Description;
            service.Unit = input.Unit;
            service.VAT = input.VAT;
            service.RetailPrice = input.RetailPrice;
            service.WholesalePrice = input.WholesalePrice;

            await _serviceService.UpdateAsync(service);

            TempData["SuccessMessage"] = "Η υπηρεσία ενημερώθηκε επιτυχώς.";
            return RedirectToAction(nameof(Index));
        }

        // ----------------------- DETAILS ---------------------
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var service = await _serviceService.GetByIdForUserAsync(id, CurrentUserId);
            return service == null ? NotFound() : View(service);
        }

        // ----------------------- DELETE (GET) ----------------
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var service = await _serviceService.GetByIdForUserAsync(id, CurrentUserId);
            if (service == null) return NotFound();
            return View(service);
        }

        // ----------------------- DELETE (POST) ---------------
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var service = await _serviceService.GetByIdForUserAsync(id, CurrentUserId);
            if (service == null) return NotFound();

            await _serviceService.DeleteAsync(service);
            TempData["SuccessMessage"] = "Η υπηρεσία διαγράφηκε.";
            return RedirectToAction(nameof(Index));
        }

        // ======================= DROPDOWNS ===================

        private void SetUnits(string? selected = null)
        {
            var units = new List<string> { "ώρα", "ημέρα", "μήνας", "έργο", "τεμάχιο" };
            ViewBag.Units = new SelectList(units, selected);
        }

        /// <summary>
        /// Δίνει dropdown για ΦΠΑ με ετικέτες 0%,6%,13%,24% αλλά values αριθμητικά (string) ώστε να δένουν με decimal VAT.
        /// </summary>
        private void SetVATs(decimal? selected = null)
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem("0%",  "0"),
                new SelectListItem("6%",  "6"),
                new SelectListItem("13%","13"),
                new SelectListItem("24%","24"),
            };

            if (selected.HasValue)
            {
                var sel = selected.Value.ToString("0.##", CultureInfo.InvariantCulture);
                foreach (var it in items)
                    it.Selected = it.Value == sel;
            }

            // Προσοχή: Τα Views πρέπει να δένουν με αυτό το όνομα (VATs)
            ViewBag.VATs = items;
        }

        // ======================= HELPERS =====================

        // Υπολογισμός επόμενου κωδικού Υπηρεσίας (0001, 0002, ...)
        private async Task<string> GenerateNextServiceCodeAsync(int userId)
        {
            var allForUser = await _serviceService.GetAllByUserAsync(userId);

            var numeric = allForUser
                .Where(s => !string.IsNullOrWhiteSpace(s.ServiceCode) &&
                            s.ServiceCode.All(char.IsDigit))
                .Select(s => int.TryParse(s.ServiceCode, out var n) ? n : 0);

            var next = (numeric.Any() ? numeric.Max() : 0) + 1;
            return next.ToString("D4");
        }
    }
}

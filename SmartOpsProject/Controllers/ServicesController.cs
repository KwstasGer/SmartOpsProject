using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartOpsProject.Models;
using SmartOpsProject.Services;
using System.Threading.Tasks;

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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var services = await _serviceService.GetAllByUserAsync(CurrentUserId);
            return View(services);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            SetUnits(); // <-- dropdown για Unit
            var model = new Service { UserId = CurrentUserId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            var uid = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (uid == 0) return RedirectToAction("Login", "Account");

            ModelState.Remove(nameof(Service.User));
            if (!ModelState.IsValid)
            {
                SetUnits(service.Unit); // <-- ξαναγέμισε dropdown με επιλεγμένη τιμή
                return View(service);
            }

            service.UserId = uid;
            await _serviceService.AddAsync(service);
            TempData["SuccessMessage"] = "Η υπηρεσία δημιουργήθηκε.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var service = await _serviceService.GetByIdForUserAsync(id, CurrentUserId);
            if (service == null) return NotFound();

            SetUnits(service.Unit); // <-- dropdown με selected
            return View(service);
        }

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
                SetUnits(input.Unit); // <-- διατήρηση επιλογής
                return View(input);
            }
            service.UserId = uid;
            service.ServiceCode = input.ServiceCode;
            service.Description = input.Description;
            service.Unit = input.Unit;
            service.VAT = input.VAT;
            service.RetailPrice = input.RetailPrice;
            service.WholesalePrice = input.WholesalePrice;

            await _serviceService.UpdateAsync(service);
            TempData["SuccessMessage"] = "Η υπηρεσία ενημερώθηκε.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var service = await _serviceService.GetByIdForUserAsync(id, CurrentUserId);
            return service == null ? NotFound() : View(service);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var service = await _serviceService.GetByIdForUserAsync(id, CurrentUserId);
            if (service == null) return NotFound();
            return View(service);
        }

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


        private void SetUnits(string? selected = null)
            {
                // Βάλε εδώ τις μονάδες που θέλεις να υποστηρίζεις
                var units = new List<string> { "ώρα", "ημέρα", "μήνας", "έργο", "τεμάχιο" };
                ViewBag.Units = new SelectList(units, selected);
            }

}
}

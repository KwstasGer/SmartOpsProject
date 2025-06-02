using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public async Task<IActionResult> Index()
        {
            var services = await _serviceService.GetAllAsync();
            return View(services);
        }

        public IActionResult Create()
        {
            SetDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (ModelState.IsValid)
            {
                service.RetailPrice ??= 0.00m;
                service.WholesalePrice ??= 0.00m;

                await _serviceService.AddAsync(service);
                TempData["SuccessMessage"] = "Η υπηρεσία δημιουργήθηκε με επιτυχία.";
                return RedirectToAction(nameof(Index));
            }

            SetDropDowns();
            return View(service);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return BadRequest();

            var service = await _serviceService.GetByIdAsync(id);
            if (service == null)
                return NotFound();

            SetDropDowns();
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                service.RetailPrice ??= 0.00m;
                service.WholesalePrice ??= 0.00m;

                await _serviceService.UpdateAsync(service);
                TempData["SuccessMessage"] = "Η υπηρεσία ενημερώθηκε με επιτυχία.";
                return RedirectToAction(nameof(Index));
            }

            SetDropDowns();
            return View(service);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var service = await _serviceService.GetByIdAsync(id);
            return service == null ? NotFound() : View(service);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _serviceService.GetByIdAsync(id);
            if (service == null)
                return NotFound();

            await _serviceService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Η υπηρεσία διαγράφηκε με επιτυχία.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return BadRequest();

            var service = await _serviceService.GetByIdAsync(id);
            return service == null ? NotFound() : View(service);
        }

        private void SetDropDowns()
        {
            ViewBag.Units = new List<SelectListItem>
            {
                new SelectListItem { Value = "Τεμάχια", Text = "Τεμάχια" },
                new SelectListItem { Value = "Κιλά", Text = "Κιλά" },
                new SelectListItem { Value = "Λίτρα", Text = "Λίτρα" },
                new SelectListItem { Value = "Μέτρα", Text = "Μέτρα" }
            };

            ViewBag.VATOptions = new List<SelectListItem>
            {
                new SelectListItem { Text = "0%", Value = "0" },
                new SelectListItem { Text = "6%", Value = "6" },
                new SelectListItem { Text = "13%", Value = "13" },
                new SelectListItem { Text = "24%", Value = "24" }
            };
        }
    }
}

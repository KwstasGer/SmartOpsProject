using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartOps.Models;
using SmartOps.Services;

namespace SmartOps.Controllers
{
    public class CustomersController : Controller
    {
        private readonly CustomerService _customerService;

        public CustomersController(CustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _customerService.GetAllAsync());
        }

        public IActionResult Create()
        {
            SetDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _customerService.AddAsync(customer);
                TempData["SuccessMessage"] = "Ο πελάτης δημιουργήθηκε με επιτυχία.";
                return RedirectToAction(nameof(Index));
            }

            SetDropDowns(customer.VatStatus, customer.CustomerCategory);
            return View(customer);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return BadRequest();

            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            SetDropDowns(customer.VatStatus, customer.CustomerCategory);
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _customerService.UpdateAsync(customer);
                TempData["SuccessMessage"] = "Ο πελάτης ενημερώθηκε με επιτυχία.";
                return RedirectToAction(nameof(Index));
            }

            SetDropDowns(customer.VatStatus, customer.CustomerCategory);
            return View(customer);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var customer = await _customerService.GetByIdAsync(id);
            return customer == null ? NotFound() : View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            await _customerService.DeleteAsync(customer);
            TempData["SuccessMessage"] = "Ο πελάτης διαγράφηκε με επιτυχία.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return BadRequest();

            var customer = await _customerService.GetByIdAsync(id);
            return customer == null ? NotFound() : View(customer);
        }

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
    }
}

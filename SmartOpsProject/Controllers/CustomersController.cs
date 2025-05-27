using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartOps.Models;
using SmartOps.Services;

namespace SmartOps.Controllers
{
    public class CustomersController : Controller
    {
        private readonly CustomerService _CustomerService;

        public CustomersController(CustomerService customerService)
        {
            _CustomerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _CustomerService.GetAllAsync();
            return View(customers);
        }

        public IActionResult Create()
        {
            ViewBag.VatStatuses = new List<SelectListItem>
            {
                new SelectListItem { Text = "Κανονικό", Value = "Κανονικό" },
                new SelectListItem { Text = "Μειωμένο", Value = "Μειωμένο" },
                new SelectListItem { Text = "Απαλλασσόμενο", Value = "Απαλλασσόμενο" }
            };

            ViewBag.CustomerCategories = new List<SelectListItem>
            {
                new SelectListItem { Text = "Λιανικής", Value = "Λιανικής" },
                new SelectListItem { Text = "Χονδρικής", Value = "Χονδρικής" }
            };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _CustomerService.AddAsync(customer);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.VatStatuses = new SelectList(
                new List<string> { "Κανονικό", "Μειωμένο", "Απαλλασσόμενο" },
                selectedValue: customer.VatStatus
            );

            ViewBag.CustomerCategories = new SelectList(
                new List<string> { "Λιανικής", "Χονδρικής" },
                selectedValue: customer.CustomerCategory
            );

            return View(customer);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _CustomerService.GetByIdAsync(id);
            if (customer == null) return NotFound();

            ViewBag.VatStatuses = new SelectList(
                new List<string> { "Κανονικό", "Μειωμένο", "Απαλλασσόμενο" },
                selectedValue: customer.VatStatus
            );

            ViewBag.CustomerCategories = new SelectList(
                new List<string> { "Λιανικής", "Χονδρικής" },
                selectedValue: customer.CustomerCategory
            );

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                await _CustomerService.UpdateAsync(customer);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.VatStatuses = new SelectList(
                new List<string> { "Κανονικό", "Μειωμένο", "Απαλλασσόμενο" },
                selectedValue: customer.VatStatus
            );

            ViewBag.CustomerCategories = new SelectList(
                new List<string> { "Λιανικής", "Χονδρικής" },
                selectedValue: customer.CustomerCategory
            );

            return View(customer);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _CustomerService.GetByIdAsync(id);
            return customer == null ? NotFound() : View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _CustomerService.GetByIdAsync(id);
            if (customer != null)
            {
                await _CustomerService.DeleteAsync(customer);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await _CustomerService.GetByIdAsync(id);
            return customer == null ? NotFound() : View(customer);
        }
    }
}

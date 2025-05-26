using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartOps.Models;
using SmartOps.Services;

namespace SmartOps.Controllers
{
    public class CustomersController : Controller
    {
        private readonly CustomerService _CustomerService;

        public CustomersController(CustomerService clientService)
        {
            _CustomerService = clientService;
        }

        public async Task<IActionResult> Index()
        {
            var clients = await _CustomerService.GetAllAsync();
            return View(clients);
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
        public async Task<IActionResult> Create(Customers client)
        {
            if (ModelState.IsValid)
            {
                await _CustomerService.AddAsync(client);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.VatStatuses = new SelectList(
                new List<string> { "Κανονικό", "Μειωμένο", "Απαλλασσόμενο" },
                selectedValue: client.VatStatus
            );

            ViewBag.CustomerCategories = new SelectList(
                new List<string> { "Λιανικής", "Χονδρικής" },
                selectedValue: client.CustomerCategory
            );

            return View(client);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = await _CustomerService.GetByIdAsync(id);
            if (client == null) return NotFound();

            ViewBag.VatStatuses = new SelectList(
                new List<string> { "Κανονικό", "Μειωμένο", "Απαλλασσόμενο" },
                selectedValue: client.VatStatus
            );

            ViewBag.CustomerCategories = new SelectList(
                new List<string> { "Λιανικής", "Χονδρικής" },
                selectedValue: client.CustomerCategory
            );

            return View(client);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customers client)
        {
            if (id != client.Id) return BadRequest();

            ViewBag.VatStatuses = new SelectList(
                new List<string> { "Κανονικό", "Μειωμένο", "Απαλλασσόμενο" },
                selectedValue: client.VatStatus
            );

            ViewBag.CustomerCategories = new SelectList(
                new List<string> { "Λιανικής", "Χονδρικής" },
                selectedValue: client.CustomerCategory
            );

            if (ModelState.IsValid)
            {
                await _CustomerService.UpdateAsync(client);
                return RedirectToAction(nameof(Index));
            }

            return View(client);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var client = await _CustomerService.GetByIdAsync(id);
            return client == null ? NotFound() : View(client);
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
            var client = await _CustomerService.GetByIdAsync(id);
            return client == null ? NotFound() : View(client);
        }
    }
}

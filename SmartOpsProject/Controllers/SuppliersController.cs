using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartOps.Data;
using SmartOpsProject.Models;

namespace SmartOpsProject.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly SmartOpsDbContext _context;

        public SuppliersController(SmartOpsDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Suppliers.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null or <= 0)
                return BadRequest();

            var supplier = await _context.Suppliers.FirstOrDefaultAsync(m => m.Id == id);
            return supplier == null ? NotFound() : View(supplier);
        }

        public IActionResult Create()
        {
            SetDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                _context.Add(supplier);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ο προμηθευτής δημιουργήθηκε με επιτυχία.";
                return RedirectToAction(nameof(Index));
            }

            SetDropDowns(supplier.VatStatus, supplier.SupplierCategory);
            return View(supplier);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null or <= 0)
                return BadRequest();

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();

            SetDropDowns(supplier.VatStatus, supplier.SupplierCategory);
            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (id != supplier.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(supplier);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Ο προμηθευτής ενημερώθηκε με επιτυχία.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await SupplierExistsAsync(supplier.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            SetDropDowns(supplier.VatStatus, supplier.SupplierCategory);
            return View(supplier); 
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null or <= 0)
                return BadRequest();

            var supplier = await _context.Suppliers.FirstOrDefaultAsync(m => m.Id == id);
            return supplier == null ? NotFound() : View(supplier);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ο προμηθευτής διαγράφηκε με επιτυχία.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> SupplierExistsAsync(int id)
        {
            return await _context.Suppliers.AnyAsync(e => e.Id == id);
        }

        private void SetDropDowns(string? selectedVat = null, string? selectedCategory = null)
        {
            ViewBag.VatStatuses = new SelectList(
                new List<string> { "Κανονικό", "Μειωμένο", "Απαλλάσσεται" },
                selectedVat
            );

            ViewBag.SupplierCategories = new SelectList(
                new List<string> { "Εσωτερικού", "ΕΕ", "Τρίτων Χωρών" },
                selectedCategory
            );
        }
    }
}

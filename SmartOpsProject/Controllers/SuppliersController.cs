using System;
using System.Linq;
using System.Threading.Tasks;
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

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Suppliers.ToListAsync());
        }

        // GET: Suppliers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supplier == null)
                return NotFound();

            return View(supplier);
        }

        // GET: Suppliers/Create
        public IActionResult Create()
        {
            SetDropDowns();
            return View();
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SupplierCode,Name,TaxIdentificationNumber,Country,Address,City,PostalCode,SupplierCategory,VatStatus")] Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                _context.Add(supplier);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            SetDropDowns();
            return View(supplier);
        }

        // GET: Suppliers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();

            SetDropDowns();
            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SupplierCode,Name,TaxIdentificationNumber,Country,Address,City,PostalCode,SupplierCategory,VatStatus")] Supplier supplier)
        {
            if (id != supplier.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(supplier);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupplierExists(supplier.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            SetDropDowns();
            return View(supplier);
        }

        // GET: Suppliers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(m => m.Id == id);

            if (supplier == null)
                return NotFound();

            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier != null)
                _context.Suppliers.Remove(supplier);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SupplierExists(int id)
        {
            return _context.Suppliers.Any(e => e.Id == id);
        }

        private void SetDropDowns()
        {
            ViewBag.SupplierCategories = new List<SelectListItem>
            {
                new SelectListItem { Text = "Εσωτερικού", Value = "Εσωτερικού" },
                new SelectListItem { Text = "ΕΕ", Value = "ΕΕ" },
                new SelectListItem { Text = "Τρίτων Χωρών", Value = "Τρίτων Χωρών" }
            };

            ViewBag.VatStatuses = new List<SelectListItem>
            {
                new SelectListItem { Text = "Κανονικά", Value = "Κανονικά" },
                new SelectListItem { Text = "Μειωμένο", Value = "Μειωμένο" },
                new SelectListItem { Text = "Απαλλάσσεται", Value = "Απαλλάσσεται" }
            };
        }
    }
}

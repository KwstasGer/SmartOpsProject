using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartOps.Data;
using SmartOps.Models;
using SmartOps.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartOps.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly SmartOpsDbContext _db;
        public InvoicesController(SmartOpsDbContext db) => _db = db;

        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

        // 1️⃣ Επιλογή τύπου (Items / Services / Purchases)
        [HttpGet]
        public IActionResult SelectType()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            return View();
        }

        // 2️⃣ Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            var invoices = await _db.Invoices
                .Where(i => i.UserId == CurrentUserId)
                .Include(i => i.Customer)
                .Include(i => i.Supplier)        // ➕ ΠΡΟΣΘΗΚΗ
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();

            return View(invoices);
        }

        // 3️⃣ Details
        public async Task<IActionResult> Details(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            var inv = await _db.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Supplier)
                .Include(i => i.Lines).ThenInclude(l => l.Item)
                .Include(i => i.Lines).ThenInclude(l => l.Service)
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == CurrentUserId);

            return inv == null ? NotFound() : View(inv);
        }

        // 4️⃣ Create (GET)
        [HttpGet]
        public async Task<IActionResult> Create(string type)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            // αποδεκτοί τύποι: Items, Services, Purchases
            if (type != "Items" && type != "Services" && type != "Purchases")
                return RedirectToAction(nameof(SelectType));

            await FillDropdownsAsync(type);

            var uid = CurrentUserId;

            var vm = new InvoiceCreateVm
            {
                InvoiceType = type
            };

            // Για Items ΚΑΙ Purchases χρησιμοποιούμε Items catalog
            if (type == "Items" || type == "Purchases")
            {
                vm.CatalogItems = await _db.Items
                    .Where(i => i.UserId == uid)
                    .OrderBy(i => i.Description)
                    .Select(i => new InvoiceCreateVm.CatalogItemVm
                    {
                        Type = "Item",
                        Id = i.Id,
                        Code = i.ItemCode,
                        Description = i.Description,
                        RetailPrice = i.RetailPrice,
                        WholesalePrice = i.WholesalePrice,
                        VatRate = i.VAT          // 0–100 (π.χ. 24)
                    }).ToListAsync();
            }
            else // Services
            {
                vm.CatalogItems = await _db.Services
                    .Where(s => s.UserId == uid)
                    .OrderBy(s => s.Description)
                    .Select(s => new InvoiceCreateVm.CatalogItemVm
                    {
                        Type = "Service",
                        Id = s.Id,
                        Code = s.ServiceCode,
                        Description = s.Description,
                        RetailPrice = s.RetailPrice,
                        WholesalePrice = s.WholesalePrice,
                        VatRate = s.VAT          // 0–100 (π.χ. 24)
                    }).ToListAsync();
            }

            return View(vm);
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceCreateVm vm)
        {
            // 1. Έλεγχος συνεδρίας
            if (CurrentUserId == 0)
                return RedirectToAction("Login", "Account");

            // 2. Έλεγχος τύπου παραστατικού
            if (vm.InvoiceType != "Items" &&
                vm.InvoiceType != "Services" &&
                vm.InvoiceType != "Purchases")
            {
                return RedirectToAction(nameof(SelectType));
            }

            // 3. Φιλτράρισμα άδειων γραμμών
            vm.Lines = vm.Lines?
                .Where(l => l.CatalogId > 0 && l.Quantity > 0)
                .ToList() ?? new();

            // 4. Κανονικοποίηση ΦΠΑ από % (24, 13, 6) σε 0.xx
            foreach (var l in vm.Lines)
            {
                if (l.VatRate > 1)
                    l.VatRate = Math.Round(l.VatRate / 100m, 4);  // 24 -> 0.24
            }

            // 5. Έλεγχος Πελάτη ή Προμηθευτή
            bool valid;
            if (vm.InvoiceType == "Purchases")
            {
                // εδώ το CustomerId είναι στην πραγματικότητα SupplierId
                valid = await _db.Suppliers
                    .AnyAsync(s => s.Id == vm.CustomerId && s.UserId == CurrentUserId);

                if (!valid)
                    ModelState.AddModelError(nameof(vm.CustomerId), "Μη έγκυρος προμηθευτής.");
            }
            else
            {
                // Items / Services => κανονικός πελάτης
                valid = await _db.Customers
                    .AnyAsync(c => c.Id == vm.CustomerId && c.UserId == CurrentUserId);

                if (!valid)
                    ModelState.AddModelError(nameof(vm.CustomerId), "Μη έγκυρος πελάτης.");
            }

            // 6. Αν έχουμε λάθη ή δεν υπάρχουν γραμμές -> επιστροφή στο View
            if (!ModelState.IsValid || vm.Lines.Count == 0)
            {
                if (vm.Lines.Count == 0)
                    ModelState.AddModelError("", "Προσθέστε τουλάχιστον μία γραμμή.");

                await FillDropdownsAsync(vm.InvoiceType);
                var uid = CurrentUserId;

                if (vm.InvoiceType == "Items" || vm.InvoiceType == "Purchases")
                {
                    vm.CatalogItems = await _db.Items
                        .Where(i => i.UserId == uid)
                        .OrderBy(i => i.Description)
                        .Select(i => new InvoiceCreateVm.CatalogItemVm
                        {
                            Type = "Item",
                            Id = i.Id,
                            Code = i.ItemCode,
                            Description = i.Description,
                            RetailPrice = i.RetailPrice,
                            WholesalePrice = i.WholesalePrice,
                            VatRate = i.VAT
                        }).ToListAsync();
                }
                else
                {
                    vm.CatalogItems = await _db.Services
                        .Where(s => s.UserId == uid)
                        .OrderBy(s => s.Description)
                        .Select(s => new InvoiceCreateVm.CatalogItemVm
                        {
                            Type = "Service",
                            Id = s.Id,
                            Code = s.ServiceCode,
                            Description = s.Description,
                            RetailPrice = s.RetailPrice,
                            WholesalePrice = s.WholesalePrice,
                            VatRate = s.VAT
                        }).ToListAsync();
                }

                return View(vm);
            }

            // 7. Αν η σειρά είναι λιανικής (τιμή ΜΕ ΦΠΑ), μετατροπή σε καθαρή
            var priceIncludesVat = vm.Series == "ΑΛΠ" || vm.Series == "ΑΠΥ";

            if (priceIncludesVat)
            {
                foreach (var l in vm.Lines)
                {
                    var rate = l.VatRate;    // εδώ είναι ΗΔΗ 0.24 / 0.13 / 0.06

                    if (rate > 0)
                    {
                        var gross = l.UnitPrice;          // τιμή ΜΕ ΦΠΑ από τον κατάλογο
                        var net = gross / (1 + rate);     // καθαρή τιμή
                        l.UnitPrice = Math.Round(net, 2); // αποθηκεύουμε καθαρή
                    }
                }
            }

            // 8. Δημιουργία παραστατικού
            var inv = new Invoice
            {
                UserId = CurrentUserId,
                Series = vm.Series,
                Number = await GetNextNumberAsync(vm.Series, vm.IssueDate.Year),
                IssueDate = vm.IssueDate,
                Year = vm.IssueDate.Year,
                CustomerId = vm.CustomerId,     // Purchases: εδώ είναι SupplierId
                PaymentMethod = vm.PaymentMethod,
                InvoiceType = vm.InvoiceType
            };

            // Αν είναι Τιμολόγιο Αγοράς, δένουμε και SupplierId
            if (vm.InvoiceType == "Purchases")
            {
                // Αγορές → μόνο SupplierId
                inv.CustomerId = null;
                inv.SupplierId = vm.CustomerId;     // εδώ είναι ο προμηθευτής
            }
            else
            {
                // Πωλήσεις / Υπηρεσίες → μόνο CustomerId
                inv.CustomerId = vm.CustomerId;
                inv.SupplierId = null;
            }

            // 9. Γραμμές παραστατικού
            inv.Lines = vm.Lines.Select(x =>
            {
                var line = new InvoiceLine
                {
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,  // ✅ σωστό όνομα
                    VatRate = x.VatRate       // ✅ εδώ είναι 0.xx
                };

                if (vm.InvoiceType == "Items" || vm.InvoiceType == "Purchases")
                {
                    line.ItemId = x.CatalogId;
                    line.ServiceId = null;
                }
                else
                {
                    line.ServiceId = x.CatalogId;
                    line.ItemId = null;
                }

                return line;
            }).ToList();

            // 10. Υπολογισμός συνόλων
            inv.RecalculateTotals();

            // 11. Αποθήκευση
            _db.Invoices.Add(inv);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", "Αποτυχία αποθήκευσης παραστατικού. " + msg);

                await FillDropdownsAsync(vm.InvoiceType);
                var uid = CurrentUserId;

                if (vm.InvoiceType == "Items" || vm.InvoiceType == "Purchases")
                {
                    vm.CatalogItems = await _db.Items
                        .Where(i => i.UserId == uid)
                        .OrderBy(i => i.Description)
                        .Select(i => new InvoiceCreateVm.CatalogItemVm
                        {
                            Type = "Item",
                            Id = i.Id,
                            Code = i.ItemCode,
                            Description = i.Description,
                            RetailPrice = i.RetailPrice,
                            WholesalePrice = i.WholesalePrice,
                            VatRate = i.VAT
                        }).ToListAsync();
                }
                else
                {
                    vm.CatalogItems = await _db.Services
                        .Where(s => s.UserId == uid)
                        .OrderBy(s => s.Description)
                        .Select(s => new InvoiceCreateVm.CatalogItemVm
                        {
                            Type = "Service",
                            Id = s.Id,
                            Code = s.ServiceCode,
                            Description = s.Description,
                            RetailPrice = s.RetailPrice,
                            WholesalePrice = s.WholesalePrice,
                            VatRate = s.VAT
                        }).ToListAsync();
                }

                return View(vm);
            }

            TempData["Ok"] = $"Καταχωρήθηκε το παραστατικό {inv.Series}-{inv.Number}/{inv.Year}.";
            return RedirectToAction(nameof(Details), new { id = inv.Id });
        }



        // 6️⃣ Delete
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            var inv = await _db.Invoices
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == CurrentUserId);

            return inv == null ? NotFound() : View(inv);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            var inv = await _db.Invoices
                .FirstOrDefaultAsync(i => i.Id == id && i.UserId == CurrentUserId);

            if (inv == null) return NotFound();

            _db.Invoices.Remove(inv);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Αποτυχία διαγραφής: " + (ex.InnerException?.Message ?? ex.Message);
                return RedirectToAction(nameof(Index));
            }

            TempData["Ok"] = $"Το παραστατικό {inv.Series}-{inv.Number}/{inv.Year} διαγράφηκε.";
            return RedirectToAction(nameof(Index));
        }

        // 7️⃣ Helpers
        private async Task<int> GetNextNumberAsync(string series, int year)
        {
            var max = await _db.Invoices
                .Where(i => i.UserId == CurrentUserId && i.Series == series && i.Year == year)
                .MaxAsync(i => (int?)i.Number) ?? 0;

            return max + 1;
        }

        private async Task FillDropdownsAsync(string type)
        {
            // Πελάτες ή Προμηθευτές στο ίδιο dropdown
            if (type == "Purchases")
            {
                // Τιμολόγιο αγορών → Προμηθευτές
                ViewBag.CustomerOptions = new SelectList(
                    await _db.Suppliers
                        .Where(s => s.UserId == CurrentUserId)
                        .OrderBy(s => s.Name)
                        .ToListAsync(),
                    "Id", "Name");
            }
            else
            {
                // Πωλήσεις / Υπηρεσίες → Πελάτες
                ViewBag.CustomerOptions = new SelectList(
                    await _db.Customers
                        .Where(c => c.UserId == CurrentUserId)
                        .OrderBy(c => c.Name)
                        .ToListAsync(),
                    "Id", "Name");
            }

            // Σειρές ανά τύπο
            System.Collections.Generic.IEnumerable<object> seriesList;

            if (type == "Services")
            {
                seriesList = new[]
                {
                    new { Value = "ΤΠΥ", Text = "Τιμολόγιο Παροχής Υπηρεσιών" },
                    new { Value = "ΑΠΥ", Text = "Απόδειξη Παροχής Υπηρεσιών" }
                };
            }
            else if (type == "Purchases")
            {
                seriesList = new[]
                {
                    new { Value = "ΤΑΓ", Text = "Τιμολόγιο Αγορών" }
                };
            }
            else // Items (πωλήσεις)
            {
                seriesList = new[]
                {
                    new { Value = "ΤΔΑ", Text = "Τιμολόγιο - Δελτίο Αποστολής" },
                    new { Value = "ΔΑ",  Text = "Δελτίο Αποστολής" },
                    new { Value = "ΑΛΠ", Text = "Απόδειξη Λιανικής Πώλησης" }
                };
            }

            ViewBag.SeriesOptions = new SelectList(seriesList, "Value", "Text");
        }
    }
}

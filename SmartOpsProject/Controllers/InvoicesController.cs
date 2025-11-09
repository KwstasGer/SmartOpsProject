using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartOps.Services;
using SmartOps.ViewModels;
using SmartOpsProject.Models;
using Microsoft.AspNetCore.Http; //για Session

namespace SmartOps.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly InvoiceService _invoiceService;
        private readonly CustomerService _customerService;
        private readonly ItemService _itemService;

        public InvoicesController(InvoiceService invoiceService, CustomerService customerService, ItemService itemService)
        {
            _invoiceService = invoiceService;
            _customerService = customerService;
            _itemService = itemService;
        }

        // 🔹 Helper property: το userId από Session
        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

        private async Task FillDropdownsAsync(InvoiceCreateVm vm)
        {
            var customers = await _customerService.GetAllByUserAsync(CurrentUserId);
            vm.Customers = customers.OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });

            var items = await _itemService.GetAllAsync();
            vm.Items = items.OrderBy(i => i.Description)
                .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Description });

            ViewBag.VATOptions = new[]
            {
                new SelectListItem { Text = "0%",  Value = "0"  },
                new SelectListItem { Text = "6%",  Value = "6"  },
                new SelectListItem { Text = "13%", Value = "13" },
                new SelectListItem { Text = "24%", Value = "24" }
            };
        }

        private void FillStaticDropdowns(InvoiceCreateVm vm)
        {
            ViewBag.SeriesOptions = new List<SelectListItem>
            {
                new SelectListItem("ΤΙΜ", "ΤΙΜ", vm.Series == "ΤΙΜ"),
                new SelectListItem("ΑΠΥ", "ΑΠΥ", vm.Series == "ΑΠΥ"),
                new SelectListItem("ΔΑΠ", "ΔΑΠ", vm.Series == "ΔΑΠ"),
            };

            ViewBag.PaymentOptions = new List<SelectListItem>
            {
                new SelectListItem("Μετρητά", ((int)PaymentMethod.Cash).ToString(),  vm.PaymentMethod == PaymentMethod.Cash),
                new SelectListItem("Κάρτα",   ((int)PaymentMethod.Card).ToString(),  vm.PaymentMethod == PaymentMethod.Card),
                new SelectListItem("Τράπεζα", ((int)PaymentMethod.Bank).ToString(),  vm.PaymentMethod == PaymentMethod.Bank),
            };
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new InvoiceCreateVm
            {
                Customers = (await _customerService.GetAllByUserAsync(CurrentUserId))
                    .OrderBy(x => x.Name)
                    .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }),
                Items = (await _itemService.GetAllAsync())
                    .OrderBy(x => x.Description)
                    .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Description })
            };

            FillStaticDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceCreateVm vm)
        {
            vm.Lines = vm.Lines
                .Where(l => l.ItemId > 0 && l.Quantity > 0 && l.UnitPrice >= 0)
                .ToList();

            if (!vm.Lines.Any())
                ModelState.AddModelError("", "Πρέπει να καταχωρηθεί τουλάχιστον μία γραμμή.");

            if (!ModelState.IsValid)
            {
                vm.Customers = (await _customerService.GetAllByUserAsync(CurrentUserId))
                    .OrderBy(x => x.Name)
                    .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name });

                vm.Items = (await _itemService.GetAllAsync())
                    .OrderBy(x => x.Description)
                    .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Description });

                FillStaticDropdowns(vm);
                return View(vm);
            }

            var year = vm.IssueDate.Year;
            var nextNumber = await _invoiceService.GetNextNumberAsync(CurrentUserId, vm.Series, year);

            var inv = new Invoice
            {
                UserId = CurrentUserId,   // 🔹 Σφραγίζουμε τον ιδιοκτήτη
                Series = vm.Series,
                Number = nextNumber,
                IssueDate = vm.IssueDate,
                Year = year,
                CustomerId = vm.CustomerId,
                PaymentMethod = vm.PaymentMethod
            };

            decimal totalNet = 0, totalVat = 0, totalGross = 0;

            foreach (var l in vm.Lines)
            {
                var lineNet = l.Quantity * l.UnitPrice;
                var lineVat = System.Math.Round(lineNet * (l.VatRate / 100m), 2);
                var lineGross = lineNet + lineVat;

                inv.Items.Add(new InvoiceItem
                {
                    ItemId = l.ItemId,
                    Description = string.IsNullOrWhiteSpace(l.Description) ? "" : l.Description,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    VatRate = l.VatRate,
                    LineNet = lineNet,
                    LineVat = lineVat,
                    LineGross = lineGross
                });

                totalNet += lineNet;
                totalVat += lineVat;
                totalGross += lineGross;
            }

            inv.TotalNet = totalNet;
            inv.TotalVat = totalVat;
            inv.TotalGross = totalGross;

            await _invoiceService.AddAsync(inv);

            TempData["SuccessMessage"] =
                $"Το παραστατικό {inv.Series}-{inv.Number}/{inv.Year} καταχωρήθηκε επιτυχώς.";
            return RedirectToAction(nameof(Details), new { id = inv.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var inv = await _invoiceService.GetByIdWithItemsForUserAsync(id, CurrentUserId);
            if (inv == null) return NotFound();
            return View(inv);
        }

        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var invoices = await _invoiceService.GetAllByUserAsync(CurrentUserId);
            return View(invoices);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartOps.Models;
using SmartOps.Services;
using SmartOpsProject.Models;
using System.Threading.Tasks;

namespace SmartOps.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ItemService _itemService;

        public ItemsController(ItemService itemService)
        {
            _itemService = itemService;
        }

        private int CurrentUserId => HttpContext.Session.GetInt32("UserId") ?? 0;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var items = await _itemService.GetAllByUserAsync(CurrentUserId);
            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            SetUnits(); // 👈
            var model = new Item { UserId = CurrentUserId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Item item)
        {
            var uid = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (uid == 0) return RedirectToAction("Login", "Account");

            ModelState.Remove(nameof(Item.User));
            if (!ModelState.IsValid)
            {
                SetUnits(item.Unit);
                return View(item);
            }

            item.UserId = uid;
            await _itemService.AddAsync(item);
            TempData["SuccessMessage"] = "Το είδος δημιουργήθηκε.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var item = await _itemService.GetByIdForUserAsync(id, CurrentUserId);
            if (item == null) return NotFound();
            SetUnits(item.Unit);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item input)
        {
            var uid = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (uid == 0) return RedirectToAction("Login", "Account");
            if (id != input.Id) return BadRequest();

            var item = await _itemService.GetByIdForUserAsync(id, uid);
            if (item == null) return NotFound();

            ModelState.Remove(nameof(Item.User));
            if (!ModelState.IsValid)
                SetUnits(input.Unit);
            return View(input);

            item.UserId = uid;
            item.ItemCode = input.ItemCode;
            item.Description = input.Description;
            item.Unit = input.Unit;
            item.VAT = input.VAT;
            item.RetailPrice = input.RetailPrice;
            item.WholesalePrice = input.WholesalePrice;
            item.ImagePath = input.ImagePath;

            await _itemService.UpdateAsync(item);
            TempData["SuccessMessage"] = "Το είδος ενημερώθηκε.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var item = await _itemService.GetByIdForUserAsync(id, CurrentUserId);
            return item == null ? NotFound() : View(item);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var item = await _itemService.GetByIdForUserAsync(id, CurrentUserId);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var item = await _itemService.GetByIdForUserAsync(id, CurrentUserId);
            if (item == null) return NotFound();

            await _itemService.DeleteAsync(item);
            TempData["SuccessMessage"] = "Το είδος διαγράφηκε.";
            return RedirectToAction(nameof(Index));
        }

        private void SetUnits(string? selected = null)
        {
            var units = new List<string> { "τεμάχιο", "κιλό", "λίτρο", "μέτρο", "κουτί" };
            ViewBag.Units = new SelectList(units, selected);
        }


    }
}

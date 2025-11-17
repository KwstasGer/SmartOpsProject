using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SmartOps.Models;
using SmartOps.Services;
using System.Collections.Generic;
using System.Linq;
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

        // ----------------------- INDEX -----------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var items = await _itemService.GetAllByUserAsync(CurrentUserId);
            return View(items);
        }

        // ----------------------- CREATE (GET) ----------------
        [HttpGet]
        public IActionResult Create()
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            SetUnits();
            SetVATs(); // dropdown ΦΠΑ

            var model = new Item
            {
                UserId = CurrentUserId,
                ItemCode = "*" // προτείνουμε auto
            };
            return View(model);
        }

        // ----------------------- CREATE (POST) ---------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ItemCode,Description,Unit,VAT,RetailPrice,WholesalePrice")]
            Item item)
        {
            var uid = CurrentUserId;
            if (uid == 0) return RedirectToAction("Login", "Account");

            // navigation prop δεν έρχεται από τη φόρμα
            ModelState.Remove(nameof(Item.User));

            item.UserId = uid;

            // Auto-code αν * ή κενό / παύλα
            var code = item.ItemCode?.Trim();
            if (string.IsNullOrWhiteSpace(code) || code == "*" || code == "-")
                item.ItemCode = await GenerateNextItemCodeAsync(uid);

            if (!ModelState.IsValid)
            {
                SetUnits(item.Unit);
                SetVATs(item.VAT);
                return View(item);
            }

            try
            {
                await _itemService.AddAsync(item);
                TempData["SuccessMessage"] = "Το είδος δημιουργήθηκε.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
            {
                // Μοναδικότητα ItemCode
                ModelState.AddModelError(nameof(Item.ItemCode), "Ο κωδικός είδους χρησιμοποιείται ήδη.");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "Σφάλμα βάσης: " + (ex.InnerException?.Message ?? ex.Message));
            }

            SetUnits(item.Unit);
            SetVATs(item.VAT);
            return View(item);
        }

        // ----------------------- EDIT (GET) ------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");

            var item = await _itemService.GetByIdForUserAsync(id, CurrentUserId);
            if (item == null) return NotFound();

            SetUnits(item.Unit);
            SetVATs(item.VAT);
            return View(item);
        }

        // ----------------------- EDIT (POST) -----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            // Δεν επιτρέπουμε αλλαγή ItemCode στο Edit
            [Bind("Id,Description,Unit,VAT,RetailPrice,WholesalePrice")]
            Item input)
        {
            var uid = CurrentUserId;
            if (uid == 0) return RedirectToAction("Login", "Account");
            if (id != input.Id) return BadRequest();

            var item = await _itemService.GetByIdForUserAsync(id, uid);
            if (item == null) return NotFound();

            ModelState.Remove(nameof(Item.User));
            ModelState.Remove(nameof(Item.ItemCode)); // σταθερό στο edit

            if (!ModelState.IsValid)
            {
                // να φαίνεται ο κωδικός στο view (read-only)
                input.ItemCode = item.ItemCode;
                SetUnits(input.Unit);
                SetVATs(input.VAT);
                return View(input);
            }

            // ενημέρωση πεδίων
            item.UserId = uid;
            item.Description = input.Description;
            item.Unit = input.Unit;
            item.VAT = input.VAT;
            item.RetailPrice = input.RetailPrice;
            item.WholesalePrice = input.WholesalePrice;

            await _itemService.UpdateAsync(item);
            TempData["SuccessMessage"] = "Το είδος ενημερώθηκε.";
            return RedirectToAction(nameof(Index));
        }

        // ----------------------- DETAILS ---------------------
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var item = await _itemService.GetByIdForUserAsync(id, CurrentUserId);
            return item == null ? NotFound() : View(item);
        }

        // ----------------------- DELETE (GET) ----------------
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (CurrentUserId == 0) return RedirectToAction("Login", "Account");
            var item = await _itemService.GetByIdForUserAsync(id, CurrentUserId);
            if (item == null) return NotFound();
            return View(item);
        }

        // ----------------------- DELETE (POST) ---------------
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

        // ----------------------- Helpers ---------------------
        private void SetUnits(string? selected = null)
        {
            var units = new List<string> { "τεμάχιο", "κιλό", "λίτρο", "μέτρο", "κουτί" };
            ViewBag.Units = new SelectList(units, selected);
        }

        private void SetVATs(decimal? selected = null)
        {
            // Ετικέτες με % για καθαρό UI – values αριθμητικά για σωστό binding σε decimal
            var vatItems = new List<SelectListItem>
            {
                new SelectListItem("0%",  "0"),
                new SelectListItem("6%",  "6"),
                new SelectListItem("13%", "13"),
                new SelectListItem("24%", "24")
            };

            if (selected.HasValue)
            {
                foreach (var x in vatItems)
                    x.Selected = decimal.TryParse(x.Value, out var v) && v == selected.Value;
            }

            ViewBag.VATs = vatItems;
        }

        private async Task<string> GenerateNextItemCodeAsync(int userId)
        {
            var allForUser = await _itemService.GetAllByUserAsync(userId);

            // μόνο καθαρά αριθμητικοί κωδικοί
            var numeric = allForUser
                .Where(i => !string.IsNullOrWhiteSpace(i.ItemCode) && i.ItemCode!.All(char.IsDigit))
                .Select(i => int.TryParse(i.ItemCode, out var n) ? n : 0);

            var next = (numeric.Any() ? numeric.Max() : 0) + 1;
            return next.ToString("D5"); // 00001, 00002, ...
        }
    }
}

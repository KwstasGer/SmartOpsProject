using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartOps.Models;
using SmartOps.Services;
using System.IO;

namespace SmartOps.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ItemService _itemService;
        private readonly IWebHostEnvironment _env;

        public ItemsController(ItemService itemService, IWebHostEnvironment env)
        {
            _itemService = itemService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _itemService.GetAllAsync();
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return BadRequest();

            var item = await _itemService.GetByIdAsync(id);
            return item == null ? NotFound() : View(item);
        }

        public IActionResult Create()
        {
            SetDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Item item, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                item.RetailPrice ??= 0.00m;
                item.WholesalePrice ??= 0.00m;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var path = Path.Combine(_env.WebRootPath, "images", "products", fileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    await imageFile.CopyToAsync(stream);
                    item.ImagePath = "/images/products/" + fileName;
                }

                await _itemService.AddAsync(item);
                TempData["SuccessMessage"] = "Το είδος δημιουργήθηκε με επιτυχία.";
                return RedirectToAction(nameof(Index));
            }

            SetDropDowns();
            return View(item);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return BadRequest();

            var item = await _itemService.GetByIdAsync(id);
            if (item == null)
                return NotFound();

            SetDropDowns();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item item, IFormFile? imageFile)
        {
            if (id != item.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                item.RetailPrice ??= 0.00m;
                item.WholesalePrice ??= 0.00m;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var path = Path.Combine(_env.WebRootPath, "images", "products", fileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    await imageFile.CopyToAsync(stream);
                    item.ImagePath = "/images/products/" + fileName;
                }

                await _itemService.UpdateAsync(item);
                TempData["SuccessMessage"] = "Το είδος ενημερώθηκε με επιτυχία.";
                return RedirectToAction(nameof(Index));
            }

            SetDropDowns();
            return View(item);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var item = await _itemService.GetByIdAsync(id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _itemService.GetByIdAsync(id);
            if (item == null)
                return NotFound();

            await _itemService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Το είδος διαγράφηκε με επιτυχία.";
            return RedirectToAction(nameof(Index));
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

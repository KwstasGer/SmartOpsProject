using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartOps.Models;
using SmartOps.Services;
using System.IO;
using System.Threading.Tasks;

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

        // GET: /Items
        public async Task<IActionResult> Index()
        {
            var items = await _itemService.GetAllAsync();
            return View(items);
        }

        // GET: /Items/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var item = await _itemService.GetByIdAsync(id);
            if (item == null)
                return NotFound();

            return View(item);
        }

        // GET: /Items/Create
        public IActionResult Create()
        {
            ViewBag.Units = GetUnits();
            ViewBag.VATOptions = GetVATOptions();
            return View();
        }

        // POST: /Items/Create
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

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    item.ImagePath = "/images/products/" + fileName;
                }
                else
                {
                    item.ImagePath = null;
                }

                await _itemService.AddAsync(item);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Units = GetUnits();
            ViewBag.VATOptions = GetVATOptions();
            return View(item);
        }

        // GET: /Items/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _itemService.GetByIdAsync(id);
            if (item == null)
                return NotFound();

            ViewBag.Units = GetUnits();
            ViewBag.VATOptions = GetVATOptions();
            return View(item);
        }

        // POST: /Items/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item item, IFormFile imageFile)
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

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    item.ImagePath = "/images/products/" + fileName;
                }
                else
                {
                    item.ImagePath = null;
                }

                await _itemService.UpdateAsync(item);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Units = GetUnits();
            ViewBag.VATOptions = GetVATOptions();
            return View(item);
        }

        // GET: /Items/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _itemService.GetByIdAsync(id);
            if (item == null)
                return NotFound();

            return View(item);
        }

        // POST: /Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _itemService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private List<SelectListItem> GetUnits()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Τεμάχια", Text = "Τεμάχια" },
                new SelectListItem { Value = "Κιλά", Text = "Κιλά" },
                new SelectListItem { Value = "Λίτρα", Text = "Λίτρα" },
                new SelectListItem { Value = "Μέτρα", Text = "Μέτρα" }
            };
        }

        private List<SelectListItem> GetVATOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "0%", Value = "0" },
                new SelectListItem { Text = "6%", Value = "6" },
                new SelectListItem { Text = "13%", Value = "13" },
                new SelectListItem { Text = "24%", Value = "24" }
            };
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartOps.Models;
using SmartOps.Services;
using System.IO;
using System.Threading.Tasks;

namespace SmartOps.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductService _productService;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ProductService productService, IWebHostEnvironment env)
        {
            _productService = productService;
            _env = env;
        }

        // GET: /Products
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // GET: /Products/Create
        public IActionResult Create()
        {
            ViewBag.Units = GetUnits();
            ViewBag.VATOptions = GetVATOptions();
            return View();
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                // Αν δεν δόθηκαν τιμές, βεβαιωνόμαστε ότι μπαίνουν 0.00
                if (product.RetailPrice == null)
                    product.RetailPrice = 0.00m;

                if (product.WholesalePrice == null)
                    product.WholesalePrice = 0.00m;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var path = Path.Combine(_env.WebRootPath, "images", "products", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.ImagePath = "/images/products/" + fileName;
                }

                else
                {
                    product.ImagePath = null; // ή null αν προτιμάς
                }
                

                await _productService.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in ModelState)
            {
                foreach (var subError in error.Value.Errors)
                {
                    Console.WriteLine($"[Model Error] Field: {error.Key} → {subError.ErrorMessage}");
                }
            }

            ViewBag.Units = GetUnits();
            ViewBag.VATOptions = GetVATOptions();
            return View(product);
        }

        // GET: /Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.Units = GetUnits();
            ViewBag.VATOptions = GetVATOptions();
            return View(product);
        }

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile imageFile)
        {
            if (id != product.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                if (product.RetailPrice == null)
                    product.RetailPrice = 0.00m;

                if (product.WholesalePrice == null)
                    product.WholesalePrice = 0.00m;

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    var path = Path.Combine(_env.WebRootPath, "images", "products", fileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.ImagePath = "/images/products/" + fileName;
                }

                else
                {
                    product.ImagePath = null; // ή null αν προτιμάς
                }

                await _productService.UpdateAsync(product);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Units = GetUnits();
            ViewBag.VATOptions = GetVATOptions();
            return View(product);
        }

        // GET: /Products/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: /Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteAsync(id);
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

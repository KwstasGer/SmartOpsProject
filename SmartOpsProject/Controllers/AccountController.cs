using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartOps.Services;
using SmartOps.ViewModels;
using SmartOps.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SmartOps.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;

        public AccountController(UserService userService)
        {
            _userService = userService;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View(new LoginViewModel
            {
                Date = DateTime.Today
            });
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetByEmailAsync(model.Email);
            if (user == null || !PasswordHelper.VerifyPassword(user.PasswordHash, model.Password))
            {
                ModelState.AddModelError(string.Empty, "Λάθος email ή κωδικός.");
                return View(model);
            }
            // ✅ Πρώτα καθαρίζουμε, μετά γράφουμε ΟΛΑ τα session keys
            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32("UserId", user.Id);

            // Δεν αναφερόμαστε σε user.Email γιατί δεν υπάρχει στο μοντέλο.
            // Χρησιμοποιούμε το model.Email (από το form)
            HttpContext.Session.SetString("UserEmail", model.Email);

            // Για εμφάνιση ονόματος: προτίμησε Username αν υπάρχει, αλλιώς το email του form
            HttpContext.Session.SetString("UserName",
                string.IsNullOrWhiteSpace(user.Username) ? model.Email : user.Username);

            // Αν κρατάς και ημερομηνία
            HttpContext.Session.SetString("SelectedDate", model.Date.ToString("yyyy-MM-dd"));


            TempData["SuccessMessage"] = "Επιτυχής σύνδεση.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Logout  (κρατάμε GET για να δουλεύει ο υπάρχων σύνδεσμος στο layout)
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Αποσυνδεθήκατε επιτυχώς.";
            return RedirectToAction("Login");
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // var existingUser = await _userService.GetByEmailAsync(model.Email);
            // if (existingUser != null) 
                // Ο έλεγχος ύπαρξης ας γίνει στο service με norm email
            var created = await _userService.RegisterAsync(model.Email, model.Password);
            if (!created)
            {
                ModelState.AddModelError("Email", "Αυτό το email χρησιμοποιείται ήδη.");
                return View(model);
            }

            await _userService.RegisterAsync(model.Email, model.Password);
            TempData["SuccessMessage"] = "Επιτυχής εγγραφή! Μπορείτε να συνδεθείτε.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Ο χρήστης δεν βρέθηκε.");
                return View(model);
            }

            user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
            await _userService.UpdateAsync(user);

            TempData["SuccessMessage"] = "Ο κωδικός άλλαξε με επιτυχία.";
            return RedirectToAction("Login");
        }
    }
}

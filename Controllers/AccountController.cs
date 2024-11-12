using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
#pragma warning disable CS8602 // Déréférencement d'une éventuelle référence null.
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
#pragma warning restore CS8602 // Déréférencement d'une éventuelle référence null.
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

#pragma warning disable CS8604 // Existence possible d'un argument de référence null.
            var user = await _userManager.FindByEmailAsync(model.Email);
#pragma warning restore CS8604 // Existence possible d'un argument de référence null.
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

#pragma warning disable CS8604 // Existence possible d'un argument de référence null.
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
#pragma warning restore CS8604 // Existence possible d'un argument de référence null.
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Dashboard"); // Redirect after successful login
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View(model);
            }

#pragma warning disable CS8604 // Existence possible d'un argument de référence null.
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
#pragma warning restore CS8604 // Existence possible d'un argument de référence null.
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "An account with this email already exists.");
                return View(model);
            }

#pragma warning disable CS8601 // Existence possible d'une assignation de référence null.
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };
#pragma warning restore CS8601 // Existence possible d'une assignation de référence null.

#pragma warning disable CS8604 // Existence possible d'un argument de référence null.
            var result = await _userManager.CreateAsync(user, model.Password);
#pragma warning restore CS8604 // Existence possible d'un argument de référence null.
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Dashboard"); // Redirect to Dashboard after successful registration
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken] // Protection against CSRF attacks
        public async Task<IActionResult> Logout()
        {
#pragma warning disable CS8602 // Déréférencement d'une éventuelle référence null.
            if (User.Identity.IsAuthenticated)
            {
                await _signInManager.SignOutAsync();
            }
#pragma warning restore CS8602 // Déréférencement d'une éventuelle référence null.
            return RedirectToAction("Index", "Home");
        }
    }
}

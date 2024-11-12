using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: Profile/GetProfile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

#pragma warning disable CS8601 // Existence possible d'une assignation de référence null.
            var profileModel = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
            };
#pragma warning restore CS8601 // Existence possible d'une assignation de référence null.

            return View(profileModel);
        }

        // GET: Profile/UpdateProfile
        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

#pragma warning disable CS8601 // Existence possible d'une assignation de référence null.
            var profileModel = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
            };
#pragma warning restore CS8601 // Existence possible d'une assignation de référence null.

            return View(profileModel);
        }

        // POST: Profile/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Vérification si l'email a changé et est déjà utilisé
            if (user.Email != model.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                    return View(model);
                }
            }

            // Mettre à jour les informations de l'utilisateur
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Votre profil a été mis à jour avec succès.";
                return RedirectToAction("GetProfile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}
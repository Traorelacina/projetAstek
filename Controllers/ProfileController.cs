using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebApplication1.Models;
using System.Threading.Tasks;
using System.IO;
using System;

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

        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Utilisateur non trouvé");
            }

            var model = new UpdateProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Utilisateur non trouvé");
            }

            var model = new UpdateProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel model, IFormFile profileImage)
        {
            // Supprimer les erreurs de validation des champs de mot de passe si pas de nouveau mot de passe
            if (string.IsNullOrEmpty(model.NewPassword))
            {
                ModelState.Remove("NewPassword");
                ModelState.Remove("ConfirmNewPassword");
                ModelState.Remove("CurrentPassword");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                ModelState.AddModelError("", "Utilisateur non trouvé");
                return View(model);
            }

            // Mise à jour des informations de base
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;

            // Gestion de l'upload d'image de profil
            if (profileImage != null && profileImage.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await profileImage.CopyToAsync(memoryStream);
                user.ProfilePicture = Convert.ToBase64String(memoryStream.ToArray());
            }

            // Gestion du changement de mot de passe
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Le mot de passe actuel est requis pour changer le mot de passe");
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.ConfirmNewPassword) || model.NewPassword != model.ConfirmNewPassword)
                {
                    ModelState.AddModelError("ConfirmNewPassword", "Les mots de passe ne correspondent pas");
                    return View(model);
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profil mis à jour avec succès";
                return RedirectToAction(nameof(GetProfile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }
}
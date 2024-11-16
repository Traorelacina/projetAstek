using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using MongoDB.Driver;
namespace WebApplication1.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMongoCollection<Article> _articleCollection;
        public DashboardController(UserManager<ApplicationUser> userManager, IMongoCollection<Article> articleCollection)
        {
            _userManager = userManager;
            _articleCollection = articleCollection;
        }
        // Action pour afficher le tableau de bord
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Utilisateur non trouvé");
            }
            var articles = await _articleCollection
                .Find(a => a.AuthorId == user.Id)
                .ToListAsync();
            var viewModel = new DashboardViewModel
            {
                Profile = new ProfileViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                },
                Articles = articles
            };
            return View(viewModel);
        }
        // Action pour afficher le formulaire de création d'article
        public IActionResult CreateArticle()
        {
            return View();
        }
        // Action pour traiter le formulaire de création d'article
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateArticle(Article article)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("Utilisateur non authentifié");
                }
                article.AuthorId = user.Id; // Assigner l'ID de l'utilisateur connecté
                article.CreatedAt = DateTime.UtcNow; // Ajouter la date de création
                try
                {
                    await _articleCollection.InsertOneAsync(article); // Enregistrer l'article dans MongoDB
                    TempData["SuccessMessage"] = "L'article a été créé avec succès.";
                    return RedirectToAction("Index", "Articles"); // Rediriger vers la liste des articles
                }
                catch (Exception ex)
                {
                    // Si une erreur survient lors de l'insertion dans MongoDB
                    TempData["ErrorMessage"] = "Une erreur est survenue lors de la création de l'article : " + ex.Message;
                    return View(article);
                }
            }
            // Si le modèle est invalide, afficher les erreurs dans la vue
            return View(article);
        }
        // Action pour afficher le formulaire d'édition d'article
        public async Task<IActionResult> EditArticle(string id)
        {
            var article = await _articleCollection
                .Find(a => a.Id == id)
                .FirstOrDefaultAsync();
            if (article == null)
            {
                return NotFound("Article non trouvé");
            }
            return View(article);
        }
        // Action pour traiter le formulaire d'édition d'article
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditArticle(string id, Article article)
        {
            if (id != article.Id)
            {
                return NotFound("Article non trouvé");
            }
            if (ModelState.IsValid)
            {
                var existingArticle = await _articleCollection
                    .Find(a => a.Id == id)
                    .FirstOrDefaultAsync();
                if (existingArticle == null)
                {
                    return NotFound("Article non trouvé");
                }
                existingArticle.Title = article.Title;
                existingArticle.Content = article.Content;
                existingArticle.UpdatedAt = DateTime.UtcNow; // Mettre à jour la date de modification
                try
                {
                    await _articleCollection.ReplaceOneAsync(a => a.Id == id, existingArticle);
                    return RedirectToAction(nameof(Index)); // Rediriger vers la liste des articles
                }
                catch (Exception ex)
                {
                    // Si une erreur survient lors de la mise à jour dans MongoDB
                    TempData["ErrorMessage"] = "Une erreur est survenue lors de la mise à jour de l'article : " + ex.Message;
                    return View(article);
                }
            }
            return View(article); // Renvoyer à la vue si le modèle est invalide
        }
        // Action pour supprimer un article
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteArticle(string id)
        {
            var article = await _articleCollection
                .Find(a => a.Id == id)
                .FirstOrDefaultAsync();
            if (article == null)
            {
                return NotFound("Article non trouvé");
            }
            try
            {
                await _articleCollection.DeleteOneAsync(a => a.Id == id);
                TempData["SuccessMessage"] = "L'article a été supprimé avec succès.";
            }
            catch (Exception ex)
            {
                // Si une erreur survient lors de la suppression dans MongoDB
                TempData["ErrorMessage"] = "Une erreur est survenue lors de la suppression de l'article : " + ex.Message;
            }
            return RedirectToAction(nameof(Index)); // Rediriger vers la liste des articles
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.Hubs;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ArticlesController : Controller
    {
        private readonly MongoDbService _mongoDbService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ArticlesController(
            MongoDbService mongoDbService,
            IHubContext<NotificationHub> hubContext,
            UserManager<ApplicationUser> userManager)
        {
            _mongoDbService = mongoDbService;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        // Afficher la page de création d'article
        [HttpGet]
        public IActionResult CreateArticle()
        {
            return View();
        }

        // Gérer la création d'un article avec téléchargement d'image
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateArticle(Article article, IFormFile? imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            // Assigner les informations de l'auteur
            article.AuthorId = userId;
            article.AuthorFirstName = user.FirstName ?? "Inconnu";
            article.CreatedAt = DateTime.UtcNow;

            // Gestion du téléchargement d'image (facultatif)
            if (imageFile != null && imageFile.Length > 0)
            {
                if (imageFile.ContentType.StartsWith("image/"))
                {
                    // Changer le répertoire de téléchargement à wwwroot/uploads
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    Directory.CreateDirectory(uploadDir);

                    // Vérifier la taille du fichier (maximum 10 Mo)
                    if (imageFile.Length > 10 * 1024 * 1024) // 10 Mo
                    {
                        ModelState.AddModelError("imageFile", "L'image ne peut pas dépasser 10 Mo.");
                        return View(article);
                    }

                    // Générer un nom de fichier unique pour éviter les collisions
                    string fileName = $"{Path.GetFileNameWithoutExtension(imageFile.FileName)}_{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Enregistrer le chemin de l'image dans l'article
                    article.ImagePath = $"/uploads/{fileName}";
                }
                else
                {
                    ModelState.AddModelError("imageFile", "Le fichier doit être une image.");
                    return View(article);
                }
            }
            else
            {
                // Si aucune image n'est téléchargée, ImagePath restera null ou une valeur par défaut (si vous voulez en assigner une)
                article.ImagePath = null; // Optionnel, puisque `null` est déjà la valeur par défaut
            }

            if (!ModelState.IsValid)
            {
                return View(article);
            }

            // Sauvegarder l'article dans MongoDB
            await _mongoDbService.CreateArticleAsync(article);
            TempData["SuccessMessage"] = "Article créé avec succès !";
            return RedirectToAction("Index");
        }

        // Afficher la liste des articles
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var articles = await _mongoDbService.GetAllArticlesAsync();
            return View(articles);
        }

        // Afficher les détails d'un article
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var article = await _mongoDbService.GetArticleByIdAsync(id);
            if (article == null) return NotFound();

            var comments = await _mongoDbService.GetCommentsByArticleIdAsync(id);
            ViewBag.Comments = comments;
            return View(article);
        }

        // Ajouter un commentaire
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddComment(string articleId, string content)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var user = await _userManager.FindByIdAsync(userId);
    var article = await _mongoDbService.GetArticleByIdAsync(articleId);

    if (article == null || user == null) return NotFound();

    var comment = new Comment
    {
        ArticleId = articleId,
        AuthorId = userId,
        AuthorFirstName = user.FirstName ?? "Inconnu",
        AuthorLastName = user.LastName ?? "Inconnu", // Ajout du nom de famille
        Content = content,
        CreatedAt = DateTime.UtcNow
    };

    await _mongoDbService.CreateCommentAsync(comment);

    if (article.AuthorId != userId)
    {
        string commenterName = $"{user.FirstName} {user.LastName}".Trim(); // Combine prénom et nom
        await _hubContext.Clients.User(article.AuthorId)
            .SendAsync("ReceiveNotification", $"{commenterName} a commenté votre article");

        var notification = new Notification
        {
            UserId = article.AuthorId,
            Message = $"{commenterName} a commenté votre article",
            CreatedAt = DateTime.UtcNow,
            Type = "Commentaire",
            IsRead = false,
            Link = Url.Action("Details", "Articles", new { id = articleId }) 
        };
        await _mongoDbService.SaveNotificationAsync(notification);
    }

    // Récupérer les commentaires mis à jour
    var comments = await _mongoDbService.GetCommentsByArticleIdAsync(articleId);
    return PartialView("~/Views/Articles/Partial/Comments.cshtml", comments);
}


        // Gérer les likes
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ToggleLike(string articleId)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var user = await _userManager.FindByIdAsync(userId);
    var article = await _mongoDbService.GetArticleByIdAsync(articleId);

    if (article == null) return NotFound();

    // Utiliser la méthode ToggleLikeAsync
    await _mongoDbService.ToggleLikeAsync(articleId, userId);

    // Récupérer le nombre de likes mis à jour
    var likesCount = await _mongoDbService.GetLikesCountByArticleIdAsync(articleId);

    // Envoyer une notification si l'article a un autre auteur
    if (article.AuthorId != userId)
    {
        string likerName = $"{user.FirstName} {user.LastName}".Trim(); // Combine prénom et nom
        await _hubContext.Clients.User(article.AuthorId)
            .SendAsync("ReceiveNotification", $"{likerName} a aimé votre article");

        var notification = new Notification
        {
            UserId = article.AuthorId,
            Message = $"{likerName} a aimé votre article",
            CreatedAt = DateTime.UtcNow,
            Type = "Like",
            IsRead = false,
            Link = Url.Action("Details", "Articles", new { id = articleId })
        };
        await _mongoDbService.SaveNotificationAsync(notification);
    }

    // Retourner le nombre de likes mis à jour en JSON
    return Json(new { likesCount });
}

    }
}

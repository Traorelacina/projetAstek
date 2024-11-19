using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;
using WebApplication1.Models;
using WebApplication1.Hubs;
using WebApplication1.Services;
using Microsoft.AspNetCore.Antiforgery;  // Ajout du namespace pour IAntiforgery

namespace WebApplication1.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly MongoDbService _mongoDbService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserService _userService;
        private readonly IAntiforgery _antiforgery;  // Déclarer IAntiforgery

        // Injection de dépendances
        public NotificationsController(
            MongoDbService mongoDbService,
            IHubContext<NotificationHub> hubContext,
            UserService userService,  // Service UserService
            IAntiforgery antiforgery)  // Injection de IAntiforgery
        {
            _mongoDbService = mongoDbService;
            _hubContext = hubContext;
            _userService = userService;
            _antiforgery = antiforgery;  // Initialisation
        }

        // Créer une notification pour un commentaire
        [HttpPost]
        public async Task<IActionResult> CreateCommentNotification(string articleId, string content)
        {
            var article = await _mongoDbService.GetArticleByIdAsync(articleId);
            if (article == null) return NotFound("Article non trouvé");

            var commentAuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserByIdAsync(commentAuthorId);

            var comment = new Comment
            {
                ArticleId = articleId,
                Content = content,
                AuthorId = commentAuthorId,
                AuthorFirstName = user.FirstName
            };

            await _mongoDbService.CreateCommentAsync(comment);

            // Notification à l'auteur de l'article
            if (article.AuthorId != commentAuthorId)
            {
                var notificationMessage = $"{user.FirstName} {user.LastName} a commenté votre article.";
                var notificationLink = $"/Articles/Details/{articleId}";

                // Envoyer la notification via SignalR
                await _hubContext.Clients.User(article.AuthorId)
                    .SendAsync("ReceiveNotification", notificationMessage, notificationLink);

                // Sauvegarder la notification
                var notification = new Notification
                {
                    UserId = article.AuthorId,
                    Message = notificationMessage,
                    Link = notificationLink,
                    CreatedAt = DateTime.UtcNow,
                    Type = "Commentaire",
                    IsRead = false
                };

                await _mongoDbService.SaveNotificationAsync(notification);
            }

            return Ok("Commentaire ajouté et notification envoyée");
        }

        // Créer une notification pour un like
        [HttpPost]
        public async Task<IActionResult> CreateLikeNotification(string articleId)
        {
            var article = await _mongoDbService.GetArticleByIdAsync(articleId);
            if (article == null) return NotFound("Article non trouvé");

            var likerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.GetUserByIdAsync(likerId);

            // Notification à l'auteur de l'article
            if (article.AuthorId != likerId)
            {
                var notificationMessage = $"{user.FirstName} {user.LastName} a aimé votre article.";
                var notificationLink = $"/Articles/Details/{articleId}";

                // Envoyer la notification via SignalR
                await _hubContext.Clients.User(article.AuthorId)
                    .SendAsync("ReceiveNotification", notificationMessage, notificationLink);

                // Sauvegarder la notification
                var notification = new Notification
                {
                    UserId = article.AuthorId,
                    Message = notificationMessage,
                    Link = notificationLink,
                    CreatedAt = DateTime.UtcNow,
                    Type = "Like",
                    IsRead = false
                };

                await _mongoDbService.SaveNotificationAsync(notification);
            }

            return Ok("Like ajouté et notification envoyée");
        }

        // Marquer une notification comme lue
        
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> MarkAsRead(string notificationId)
{
    if (string.IsNullOrEmpty(notificationId))
    {
        return BadRequest("ID de notification invalide");
    }

    var filter = Builders<Notification>.Filter.Eq(n => n.Id, new ObjectId(notificationId));
    var update = Builders<Notification>.Update.Set(n => n.IsRead, true);

    var result = await _mongoDbService.GetCollection<Notification>("Notifications")
        .UpdateOneAsync(filter, update);

    if (result.ModifiedCount > 0)
    {
        return Ok("Notification marquée comme lue");
    }

    return NotFound("Notification non trouvée");
}


        // Afficher toutes les notifications de l'utilisateur connecté
        [HttpGet]
        [Route("Notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Utilisateur non authentifié");

            var notifications = await _mongoDbService.GetCollection<Notification>("Notifications")
                .Find(n => n.UserId == userId)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();

            // Fournir le token antiforgery pour la vue
            ViewData["AntiforgeryToken"] = _antiforgery.GetTokens(HttpContext).RequestToken;
            
            return View("GetNotifications", notifications);
        }

        // Afficher uniquement les notifications non lues
        [HttpGet]
        [Route("UnreadNotifications")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Utilisateur non authentifié");

            var unreadNotifications = await _mongoDbService.GetCollection<Notification>("Notifications")
                .Find(n => n.UserId == userId && !n.IsRead)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View("Notifications", unreadNotifications);
        }
    }
}

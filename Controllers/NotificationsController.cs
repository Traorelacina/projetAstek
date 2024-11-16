using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Claims;
using WebApplication1.Models;
using WebApplication1.Hubs;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly MongoDbService _mongoDbService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationsController(
            MongoDbService mongoDbService,
            IHubContext<NotificationHub> hubContext)
        {
            _mongoDbService = mongoDbService;
            _hubContext = hubContext;
        }

        // Créer une notification lorsqu'un commentaire est ajouté
        [HttpPost]
        public async Task<IActionResult> CreateCommentNotification(string articleId, string content)
        {
            var article = await _mongoDbService.GetArticleByIdAsync(articleId);
            if (article == null)
                return NotFound("Article non trouvé");

            var comment = new Comment
            {
                ArticleId = articleId,
                Content = content,
                AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                AuthorFirstName = User.Identity.Name
            };

            await _mongoDbService.CreateCommentAsync(comment);

            // Envoi de la notification à l'auteur de l'article
            if (article.AuthorId != null && article.AuthorId != comment.AuthorId)
            {
                await _hubContext.Clients.User(article.AuthorId)
                    .SendAsync("ReceiveNotification", $"{comment.AuthorFirstName} a commenté votre article");

                // Sauvegarde de la notification
                var notification = new Notification
                {
                    UserId = article.AuthorId,
                    Message = $"{comment.AuthorFirstName} a commenté votre article",
                    CreatedAt = DateTime.UtcNow,
                    Type = "Commentaire",
                    IsRead = false
                };
                await _mongoDbService.SaveNotificationAsync(notification);
            }

            return Ok("Commentaire ajouté et notification envoyée");
        }

        // Créer une notification lorsqu'un like est ajouté
[HttpPost]
public async Task<IActionResult> CreateLikeNotification(string articleId)
{
    var article = await _mongoDbService.GetArticleByIdAsync(articleId);
    if (article == null)
        return NotFound("Article non trouvé");

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    // Corriger la création de Like en lui passant les bons paramètres
    var like = new Like(articleId, userId); // Utilisation du constructeur avec les paramètres articleId et userId

    await _mongoDbService.CreateLikeAsync(articleId, userId);

    // Envoi de la notification à l'auteur de l'article
    if (article.AuthorId != null && article.AuthorId != userId)
    {
        await _hubContext.Clients.User(article.AuthorId)
            .SendAsync("ReceiveNotification", $"{User.Identity.Name} a aimé votre article");

        // Sauvegarde de la notification
        var notification = new Notification
        {
            UserId = article.AuthorId,
            Message = $"{User.Identity.Name} a aimé votre article",
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
        public async Task<IActionResult> MarkAsRead(string notificationId)
        {
            if (string.IsNullOrEmpty(notificationId))
                return BadRequest("ID de notification invalide");

            var filter = Builders<Notification>.Filter.Eq(n => n.Id, new ObjectId(notificationId));
            var update = Builders<Notification>.Update.Set(n => n.IsRead, true);

            var result = await _mongoDbService.GetCollection<Notification>("Notifications").UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
                return Ok("Notification marquée comme lue");

            return NotFound("Notification non trouvée");
        }

        // Afficher les notifications de l'utilisateur connecté
        [HttpGet]
        [Route("Notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Utilisateur non authentifié");

            var notifications = await _mongoDbService.GetCollection<Notification>("Notifications")
                .Find(n => n.UserId == userId)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View("GetNotifications", notifications);
        }

        // Récupérer les notifications non lues pour l'utilisateur connecté
        [HttpGet]
        [Route("UnreadNotifications")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Utilisateur non authentifié");

            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                Builders<Notification>.Filter.Eq(n => n.IsRead, false)
            );

            var unreadNotifications = await _mongoDbService.GetCollection<Notification>("Notifications")
                .Find(filter)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View("Notifications", unreadNotifications);
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using WebApplication1.Services;
using WebApplication1.Models;
using System.Threading.Tasks;

namespace WebApplication1.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly MongoDbService _mongoDbService;

        public NotificationHub(MongoDbService mongoDbService)
        {
            _mongoDbService = mongoDbService;
        }

        // Envoie une notification après l'ajout d'un article
        public async Task SendArticleNotification(string articleId)
        {
            // Récupérer l'article par ID
            var article = await _mongoDbService.GetArticleByIdAsync(articleId);
            
            if (article != null)
            {
                // Créer le message de notification
                var message = $"Un nouvel article a été ajouté : {article.Title}";

                // Enregistrer la notification dans MongoDB pour la persistance
                var notification = new Notification
                {
                    UserId = "all", // Vous pouvez adapter pour un utilisateur spécifique si nécessaire
                    Message = message,
                    CreatedAt = DateTime.UtcNow
                };
                await _mongoDbService.SaveNotificationAsync(notification);

                // Envoi de la notification en temps réel à tous les clients connectés
                await Clients.All.SendAsync("ReceiveNotification", message);
            }
        }
    }
}

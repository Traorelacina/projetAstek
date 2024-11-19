using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.Models
{
    public class Notification
    {
        [BsonId] // Indique que cette propriété est l'ID unique du document dans MongoDB
        public ObjectId Id { get; set; } // Utilisation de ObjectId comme identifiant unique

        public string UserId { get; set; } // L'utilisateur qui reçoit la notification
        public string ArticleId { get; set; } // L'ID de l'article concerné
        public string Message { get; set; } // Le message de la notification
        public DateTime CreatedAt { get; set; } // La date de la notification
        public string Type { get; set; } // Type de la notification (ex. Commentaire, Like)
        public bool IsRead { get; set; } = false;

        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string Link { get; set; }
    }
}

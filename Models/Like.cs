using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebApplication1.Models
{
    public class Like
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ArticleId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Constructeur avec les paramètres nécessaires
        public Like(string articleId, string userId)
        {
            ArticleId = articleId;
            UserId = userId;
        }
    }
}


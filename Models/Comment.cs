using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebApplication1.Models
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } // La propriété Id est optionnelle (auto-générée par MongoDB)

        public required string ArticleId { get; set; }
        public required string Content { get; set; }
        public required string AuthorId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // La propriété Author est maintenant nullable
        public ApplicationUser? Author { get; set; } // Nullable car elle peut être initialisée plus tard
    }
}

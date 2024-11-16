using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebApplication1.Models
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } // La propriété Id est auto-générée par MongoDB

        public required string ArticleId { get; set; }
        public required string Content { get; set; }
        public required string AuthorId { get; set; } // Changez 'AuthorId' pour 'UserId' si vous le souhaitez pour plus de clarté
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // La propriété Author est maintenant nullable
        public string? AuthorFirstName { get; set; }
        public string? AuthorLastName { get; set; }
    }
}

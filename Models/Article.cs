using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Article
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } 

        [Required(ErrorMessage = "Le titre est requis")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le contenu est requis")]
        public string Content { get; set; } = string.Empty;

        // Rendre AuthorFirstName nullable pour éviter les problèmes si non fourni
        public string? AuthorFirstName { get; set; } 

        public string? AuthorId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<Like> Likes { get; set; } = new List<Like>();

        [BsonIgnore]
        public ApplicationUser? Author { get; set; }

        public string? ImagePath { get; set; }

        // Méthode pour obtenir le nombre de likes
        [BsonIgnore]
        public int LikesCount => Likes?.Count ?? 0; // Compte le nombre de "likes"
    }
}

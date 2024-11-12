using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Ajoute ceci

namespace WebApplication1.Models
{
   public class Article
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }  // MongoDB le gère automatiquement

    [Required]
    public string? Title { get; set; }

    [Required]
    public string? Content { get; set; }

    // Le champ AuthorId est défini par le contrôleur, donc il n'est pas nécessaire dans le formulaire.
    public string? AuthorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();

    public ApplicationUser? Author { get; set; }
}

}

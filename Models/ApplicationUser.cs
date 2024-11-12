using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;  // Assurez-vous d'inclure cet espace de noms

namespace WebApplication1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        
        // Initialisation de la propriété Articles avec une liste vide
        public List<Article> Articles { get; set; } = new List<Article>(); 
    }
}

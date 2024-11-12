using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace WebApplication1.Models
{
    public class Like
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
#pragma warning disable CS8618 // Un champ non-nullable doit contenir une valeur autre que Null lors de la fermeture du constructeur. Envisagez d’ajouter le modificateur « required » ou de déclarer le champ comme pouvant accepter la valeur Null.
        public string Id { get; set; }
#pragma warning restore CS8618 // Un champ non-nullable doit contenir une valeur autre que Null lors de la fermeture du constructeur. Envisagez d’ajouter le modificateur « required » ou de déclarer le champ comme pouvant accepter la valeur Null.
        public required string ArticleId { get; set; }
        public required string UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

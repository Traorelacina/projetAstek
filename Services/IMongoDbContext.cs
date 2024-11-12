using MongoDB.Driver;

namespace WebApplication1.Services
{
    public interface IMongoDbContext
    {
        // Méthode générique pour obtenir une collection MongoDB
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}

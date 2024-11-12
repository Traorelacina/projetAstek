using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApplication1.Settings;
using WebApplication1.Models;

namespace WebApplication1.Services 
{
    public class MongoDbService
    {
        private readonly IMongoCollection<ApplicationUser> _usersCollection;
        private readonly IMongoDatabase _database;

        public MongoDbService(IOptions<MongoDBSettings> settings)
        {
            if (settings == null || settings.Value == null)
            {
                throw new ArgumentNullException(nameof(settings), "MongoDB settings cannot be null");
            }

            var mongoSettings = settings.Value;

            if (string.IsNullOrEmpty(mongoSettings.ConnectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty", nameof(settings));
            }

            if (string.IsNullOrEmpty(mongoSettings.DatabaseName))
            {
                throw new ArgumentException("Database name cannot be null or empty", nameof(settings));
            }

            if (string.IsNullOrEmpty(mongoSettings.UsersCollectionName))
            {
                throw new ArgumentException("Users collection name cannot be null or empty", nameof(settings));
            }

            var client = new MongoClient(mongoSettings.ConnectionString);
            _database = client.GetDatabase(mongoSettings.DatabaseName);
            _usersCollection = _database.GetCollection<ApplicationUser>(mongoSettings.UsersCollectionName);
        }

        public IMongoCollection<ApplicationUser> Users => _usersCollection;

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Collection name cannot be null or empty", nameof(name));
            }

            return _database.GetCollection<T>(name);
        }

        // Méthodes utilitaires pour les opérations courantes
        public async Task<List<T>> GetAllAsync<T>(string collectionName)
        {
            return await GetCollection<T>(collectionName)
                .Find(_ => true)
                .ToListAsync();
        }

        public async Task<T> GetByIdAsync<T>(string collectionName, string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            return await GetCollection<T>(collectionName)
                .Find(filter)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync<T>(string collectionName, T document)
        {
            await GetCollection<T>(collectionName)
                .InsertOneAsync(document);
        }

        public async Task UpdateAsync<T>(string collectionName, string id, T document)
        {
            await GetCollection<T>(collectionName)
                .ReplaceOneAsync(
                    Builders<T>.Filter.Eq("_id", id),
                    document);
        }

        public async Task DeleteAsync<T>(string collectionName, string id)
        {
            await GetCollection<T>(collectionName)
                .DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
        }

        // Méthodes spécifiques pour ApplicationUser
        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _usersCollection
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);
            return user != null;
        }
    }
}

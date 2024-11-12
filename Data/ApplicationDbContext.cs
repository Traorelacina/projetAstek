using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using WebApplication1.Models;

namespace WebApplication1
{
    public class ApplicationDbContext
    {
        private readonly IMongoDatabase _database;

        public ApplicationDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDbConnection"));
            _database = client.GetDatabase(configuration["DatabaseName"]);
        }

        public IMongoCollection<Article> Articles => _database.GetCollection<Article>("Articles");
        public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("Comments");
        public IMongoCollection<Like> Likes => _database.GetCollection<Like>("Likes");
    }
}

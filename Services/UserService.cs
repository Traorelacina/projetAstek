using MongoDB.Driver;
using WebApplication1.Models;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public class UserService
    {
        private readonly IMongoCollection<ApplicationUser> _usersCollection;

        public UserService(MongoDbService mongoDbService)
        {
            _usersCollection = mongoDbService.GetCollection<ApplicationUser>("Users"); // Assurez-vous que "Users" est la collection correcte
        }

        // Méthode pour récupérer un utilisateur par son ID
        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _usersCollection.Find(user => user.Id == userId).FirstOrDefaultAsync();
        }
    }
}

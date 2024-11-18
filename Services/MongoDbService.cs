using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApplication1.Models;
using WebApplication1.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        private readonly IMongoCollection<ApplicationUser> _usersCollection;
        private readonly IMongoCollection<Notification> _notificationsCollection;
        private readonly IMongoCollection<Article> _articlesCollection;
        private readonly IMongoCollection<Comment> _commentsCollection;
        private readonly IMongoCollection<Like> _likesCollection;

        public MongoDbService(IOptions<MongoDBSettings> settings)
        {
            var mongoSettings = settings?.Value;
            if (mongoSettings == null || string.IsNullOrEmpty(mongoSettings.ConnectionString) || string.IsNullOrEmpty(mongoSettings.DatabaseName))
            {
                throw new ArgumentException("Invalid MongoDB settings");
            }

            var client = new MongoClient(mongoSettings.ConnectionString);
            _database = client.GetDatabase(mongoSettings.DatabaseName);

            _usersCollection = _database.GetCollection<ApplicationUser>(mongoSettings.UsersCollectionName);
            _notificationsCollection = _database.GetCollection<Notification>("Notifications");
            _articlesCollection = _database.GetCollection<Article>("Articles");
            _commentsCollection = _database.GetCollection<Comment>("Comments");
            _likesCollection = _database.GetCollection<Like>("Likes");
        }

        // Méthode générique pour obtenir une collection
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }

        // Méthode pour sauvegarder une notification
        public async Task SaveNotificationAsync(Notification notification)
        {
            await _notificationsCollection.InsertOneAsync(notification);
        }

        // Méthode pour obtenir une liste paginée des articles
        public async Task<List<Article>> GetArticlesAsync(int page, int pageSize)
        {
            return await _articlesCollection
                .Find(_ => true)
                .SortByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        // Méthode pour récupérer tous les articles
        public async Task<List<Article>> GetAllArticlesAsync()
        {
            return await _articlesCollection
                .Find(_ => true)
                .SortByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        // Méthode pour obtenir le total d'articles
        public async Task<long> GetTotalArticlesAsync()
        {
            return await _articlesCollection.CountDocumentsAsync(_ => true);
        }

        // Méthode pour obtenir un article par ID
        public async Task<Article> GetArticleByIdAsync(string id)
        {
            return await _articlesCollection.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        // Méthode pour créer un article
        public async Task CreateArticleAsync(Article article)
        {
            article.CreatedAt = DateTime.UtcNow;
            article.UpdatedAt = DateTime.UtcNow;
            article.Likes = new List<Like>();
            await _articlesCollection.InsertOneAsync(article);
            
        }

        // Méthode pour mettre à jour un article
        public async Task UpdateArticleAsync(Article article)
        {
            var filter = Builders<Article>.Filter.Eq(a => a.Id, article.Id);
            var update = Builders<Article>.Update
                .Set(a => a.Title, article.Title)
                .Set(a => a.Content, article.Content)
                .Set(a => a.UpdatedAt, DateTime.UtcNow)
                .Set(a => a.Likes, article.Likes);

            await _articlesCollection.UpdateOneAsync(filter, update);
        }

        // Méthode pour supprimer un article
        public async Task DeleteArticleAsync(string id)
        {
            await _articlesCollection.DeleteOneAsync(a => a.Id == id);
            // Supprimer également les commentaires associés
            await _commentsCollection.DeleteManyAsync(c => c.ArticleId == id);
            // Supprimer les likes associés
            await _likesCollection.DeleteManyAsync(l => l.ArticleId == id);
        }

        // Méthode pour obtenir les commentaires d'un article par ID
        public async Task<List<Comment>> GetCommentsByArticleIdAsync(string articleId)
        {
            return await _commentsCollection
                .Find(c => c.ArticleId == articleId)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // Méthode pour créer un commentaire
        public async Task CreateCommentAsync(Comment comment)
        {
            comment.CreatedAt = DateTime.UtcNow;
            await _commentsCollection.InsertOneAsync(comment);
        }

        // Méthode pour supprimer un commentaire
        public async Task DeleteCommentAsync(string id)
        {
            await _commentsCollection.DeleteOneAsync(c => c.Id == id);
        }

        // Méthode pour obtenir un like d'un utilisateur pour un article
        public async Task<Like> GetLikeByUserAndArticleAsync(string userId, string articleId)
        {
            return await _likesCollection
                .Find(l => l.UserId == userId && l.ArticleId == articleId)
                .FirstOrDefaultAsync();
        }

        // Méthode pour créer un like
        public async Task CreateLikeAsync(string articleId, string userId)
        {
            var like = new Like(articleId, userId);
            await _likesCollection.InsertOneAsync(like);

            // Mettre à jour le nombre de likes dans l'article
            var article = await GetArticleByIdAsync(articleId);
            if (article != null)
            {
                article.Likes.Add(like);
                await UpdateArticleAsync(article);
            }
        }

        // Méthode pour supprimer un like
        public async Task DeleteLikeAsync(string articleId, string userId)
        {
            await _likesCollection.DeleteOneAsync(l => l.ArticleId == articleId && l.UserId == userId);

            // Mettre à jour le nombre de likes dans l'article
            var article = await GetArticleByIdAsync(articleId);
            if (article != null)
            {
                var likeToRemove = article.Likes.FirstOrDefault(l => l.UserId == userId);
                if (likeToRemove != null)
                {
                    article.Likes.Remove(likeToRemove);
                    await UpdateArticleAsync(article);
                }
            }
        }

        // Méthode pour obtenir le nombre de likes d'un article
        public async Task<long> GetLikesCountByArticleIdAsync(string articleId)
        {
            return await _likesCollection.CountDocumentsAsync(l => l.ArticleId == articleId);
        }

        // Méthode pour ajouter ou supprimer un like
        public async Task ToggleLikeAsync(string articleId, string userId)
        {
            var article = await GetArticleByIdAsync(articleId);
            if (article == null)
                throw new ArgumentException("Article not found");

            var existingLike = article.Likes.FirstOrDefault(l => l.UserId == userId);

            if (existingLike != null)
            {
                // Supprimer le like
                article.Likes.Remove(existingLike);
                await DeleteLikeAsync(articleId, userId);
            }
            else
            {
                // Ajouter le like
                var like = new Like(articleId, userId);
                article.Likes.Add(like);
                await CreateLikeAsync(articleId, userId);
            }

            await UpdateArticleAsync(article);
        }
    }
}
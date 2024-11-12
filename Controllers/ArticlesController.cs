using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
   public class ArticlesController : Controller
{
    private readonly IMongoCollection<Article> _articleCollection;
    private readonly IMongoCollection<Comment> _commentCollection;
    private readonly IMongoCollection<Like> _likeCollection;

    public ArticlesController(IMongoCollection<Article> articleCollection, IMongoCollection<Comment> commentCollection, IMongoCollection<Like> likeCollection)
    {
        _articleCollection = articleCollection;
        _commentCollection = commentCollection;
        _likeCollection = likeCollection;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        const int pageSize = 10;
        var articles = await _articleCollection
            .Find(_ => true)
            .SortByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = Math.Ceiling((double)await _articleCollection.CountDocumentsAsync(_ => true) / pageSize);

        return View(articles);
    }

    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var article = await _articleCollection
            .Find(a => a.Id == id)
            .FirstOrDefaultAsync();

        if (article == null) return NotFound();

        var comments = await _commentCollection
            .Find(c => c.ArticleId == id)
            .SortByDescending(c => c.CreatedAt)
            .ToListAsync();

        ViewBag.Comments = comments;
        ViewBag.LikesCount = await _likeCollection.CountDocumentsAsync(l => l.ArticleId == id);

        return View(article);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(string articleId, string content)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(articleId) || string.IsNullOrEmpty(content)) return BadRequest();

#pragma warning disable CS8601 // Existence possible d'une assignation de référence null.
            var comment = new Comment
        {
            ArticleId = articleId,
            Content = content,
            AuthorId = userId,
            CreatedAt = DateTime.UtcNow
        };
#pragma warning restore CS8601 // Existence possible d'une assignation de référence null.

            await _commentCollection.InsertOneAsync(comment);
        return RedirectToAction("Details", new { id = articleId });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleLike(string articleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var like = await _likeCollection
            .Find(l => l.ArticleId == articleId && l.UserId == userId)
            .FirstOrDefaultAsync();

        if (like != null)
        {
            await _likeCollection.DeleteOneAsync(l => l.Id == like.Id);
        }
        else
        {
#pragma warning disable CS8601 // Existence possible d'une assignation de référence null.
                like = new Like
            {
                ArticleId = articleId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
#pragma warning restore CS8601 // Existence possible d'une assignation de référence null.
                await _likeCollection.InsertOneAsync(like);
        }

        var likesCount = await _likeCollection.CountDocumentsAsync(l => l.ArticleId == articleId);
        return Json(new { likesCount });
    }
}

}

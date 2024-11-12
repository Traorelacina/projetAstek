using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WebApplication1.Services; 

using WebApplication1.Models;

public class UsersController : Controller
{
    private readonly IMongoCollection<ApplicationUser> _usersCollection;

    public UsersController(MongoDbService mongoDbService)
    {
#pragma warning disable CS8601 // Existence possible d'une assignation de référence null.
        _usersCollection = mongoDbService.GetCollection<ApplicationUser>("Users");
#pragma warning restore CS8601 // Existence possible d'une assignation de référence null.
    }

    public IActionResult Index()
    {
        var users = _usersCollection.Find(user => true).ToList();
        return View(users);
    }

    [HttpPost]
    public IActionResult Create(ApplicationUser user)
    {
        _usersCollection.InsertOne(user);
        return RedirectToAction("Index");
    }
}

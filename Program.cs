using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using WebApplication1.Hubs;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.Settings;

var builder = WebApplication.CreateBuilder(args);

// Configuration de MongoDB
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));
builder.Services.AddSingleton<IMongoDBSettings>(sp =>
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDBSettings>>().Value);

// Client MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IMongoDBSettings>();
    return new MongoClient(settings.ConnectionString);
});

// Base de données MongoDB
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IMongoDBSettings>();
    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddSingleton<IMongoCollection<Article>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<Article>("Articles");  // Assurez-vous que le nom de la collection est correct
});

builder.Services.AddScoped<UserService>();

// Service MongoDbService
builder.Services.AddScoped<MongoDbService>();

// Configuration de l'Antiforgery (CSRF)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN"; // Nom de l'en-tête pour transmettre le jeton
});

// Ajout de Swagger pour l'API
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
});

// Ajout de Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddUserStore<MongoUserStore>() // Vérifiez que MongoUserStore est bien défini
    .AddRoleStore<MongoRoleStore>() // Vérifiez que MongoRoleStore est bien défini
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
});

// Politique d'autorisation
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

// SignalR pour les notifications en temps réel
builder.Services.AddSignalR();

var app = builder.Build();

// Swagger en mode développement
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


// Authentification et autorisation
app.UseAuthentication();
app.UseAuthorization();

// Configuration des points de terminaison
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/notificationHub");

app.Run();

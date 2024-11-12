using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using WebApplication1.Models;
using Microsoft.Extensions.Options;
using WebApplication1.Services;
using WebApplication1.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
});

// Configure MongoDB settings
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));
builder.Services.AddSingleton<IMongoDBSettings>(sp => 
    sp.GetRequiredService<IOptions<MongoDBSettings>>().Value);

// Register MongoClient and IMongoDatabase
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IMongoDBSettings>();
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IMongoDBSettings>();
    return client.GetDatabase(settings.DatabaseName);
});

// Ajout des collections MongoDB
builder.Services.AddScoped<IMongoCollection<Article>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<Article>("Articles");
});

// Ajout de la collection Comment
builder.Services.AddScoped<IMongoCollection<Comment>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<Comment>("Comments");
});

// Ajout de la collection Like
builder.Services.AddScoped<IMongoCollection<Like>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    return database.GetCollection<Like>("Likes");
});

// Register MongoDbService
builder.Services.AddSingleton<MongoDbService>();

// Configure Identity with MongoDB store for users and roles
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddUserStore<MongoUserStore>()
    .AddRoleStore<MongoRoleStore>()
    .AddDefaultTokenProviders();

// Add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
});

// Add authorization
builder.Services.AddAuthorization();

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();

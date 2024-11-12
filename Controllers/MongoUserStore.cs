using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System.Security.Claims;
using WebApplication1.Models;
using WebApplication1.Settings;

public class MongoUserStore : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserEmailStore<ApplicationUser>
{
    private readonly IMongoCollection<ApplicationUser> users;

    public MongoUserStore(IMongoDBSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        users = database.GetCollection<ApplicationUser>(settings.UsersCollectionName);
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await users.InsertOneAsync(user, null, cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await users.DeleteOneAsync(u => u.Id == user.Id, cancellationToken);
        return IdentityResult.Success;
    }

    public void Dispose()
    {
        // Dispose of any resources if necessary
    }

    public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await users.Find(u => u.Id == userId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return await users.Find(u => u.NormalizedUserName == normalizedUserName).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id);
    }

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await users.ReplaceOneAsync(u => u.Id == user.Id, user, new ReplaceOptions(), cancellationToken);
        return IdentityResult.Success;
    }

    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        return await users.Find(u => u.NormalizedEmail == normalizedEmail).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Email);
    }

    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.EmailConfirmed);
    }

    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }
}
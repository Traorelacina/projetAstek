using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public class MongoRoleStore : IRoleStore<IdentityRole>
    {
        private readonly IMongoCollection<IdentityRole> _roles;

        public MongoRoleStore(IMongoDatabase database)
        {
            _roles = database.GetCollection<IdentityRole>("Roles");
        }

        public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            _roles.InsertOne(role);
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            _roles.ReplaceOne(r => r.Id == role.Id, role);
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            _roles.DeleteOne(r => r.Id == role.Id);
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<string?> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role?.Id);
        }

        public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role?.Name);
        }

        public Task SetRoleNameAsync(IdentityRole role, string? roleName, CancellationToken cancellationToken)
        {
            if (role != null)
            {
                role.Name = roleName;
            }
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role?.NormalizedName);
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string? normalizedName, CancellationToken cancellationToken)
        {
            if (role != null)
            {
                role.NormalizedName = normalizedName;
            }
            return Task.CompletedTask;
        }

        public async Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return await _roles.Find(r => r.Id == roleId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return await _roles.Find(r => r.NormalizedName == normalizedRoleName).FirstOrDefaultAsync(cancellationToken);
        }

        public void Dispose()
        {
            // Aucune ressource à libérer
        }
    }
}

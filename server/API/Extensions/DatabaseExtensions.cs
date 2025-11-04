using API.Database;
using API.Models.Constants;
using Role = Api.Models.DsfTables.Role;

namespace API.Extensions;

public static class DatabaseExtensions
{
    public static async Task EnsureRolesSeededAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var commandExecutor = scope.ServiceProvider.GetRequiredService<ICommandExecutor>();
        var queryExecutor = scope.ServiceProvider.GetRequiredService<IQueryExecutor>();

        var existingRoles = (await queryExecutor.GetAllAsync<Role>()).ToList();

        var missingRoles = RoleNames.GetAll()
            .Where(enumRole => existingRoles.All(r => r.RoleId != enumRole.Id))
            .Select(enumRole => new Role
            {
                RoleId = enumRole.Id,
                RoleName = enumRole.Name
            })
            .ToList();

        if (missingRoles.Count != 0)
            await commandExecutor.BulkInsertAsync(missingRoles, CancellationToken.None);
    }
}
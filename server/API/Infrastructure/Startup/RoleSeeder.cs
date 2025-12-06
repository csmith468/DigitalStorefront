using API.Database;
using API.Models.Constants;
using Api.Models.DsfTables;

namespace API.Infrastructure.Startup;

public interface IRoleSeeder
{
    Task EnsureRolesSeededAsync(CancellationToken ct);
}

public class RoleSeeder : IRoleSeeder
{
    private readonly ICommandExecutor _commandExecutor;
    private readonly IQueryExecutor _queryExecutor;

    public RoleSeeder(ICommandExecutor commandExecutor, IQueryExecutor queryExecutor)
    {
        _commandExecutor = commandExecutor;
        _queryExecutor = queryExecutor;
    }
    public async Task EnsureRolesSeededAsync(CancellationToken ct)
    {
        var existingRoles = (await _queryExecutor.GetAllAsync<Role>(ct)).ToList();

        var missingRoles = RoleNames.GetAll()
            .Where(enumRole => existingRoles.All(r => r.RoleId != enumRole.Id))
            .Select(enumRole => new Role
            {
                RoleId = enumRole.Id,
                RoleName = enumRole.Name
            })
            .ToList();

        if (missingRoles.Count != 0)
            await _commandExecutor.BulkInsertAsync(missingRoles, ct);
    }
}
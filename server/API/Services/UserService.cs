using API.Database;
using API.Models.DsfTables;

namespace API.Services;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(int id, CancellationToken ct = default);
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken ct = default);
}

public class UserService : IUserService
{
    private readonly IQueryExecutor _queryExecutor;

    public UserService(IQueryExecutor queryExecutor)
    {
        _queryExecutor = queryExecutor;
    }
    
    public async Task<User?> GetUserByIdAsync(int id, CancellationToken ct = default)
    {
        return await _queryExecutor.GetByIdAsync<User>(id, ct);
    }
    
    public async Task<User?> GetUserByUsernameAsync(string username, CancellationToken ct = default)
    {
        return (await _queryExecutor.GetByFieldAsync<User>("username", username, ct)).FirstOrDefault();
    }
}
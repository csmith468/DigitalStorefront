using API.Database;
using API.Models.DsfTables;

namespace API.Services;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string username);
}

public class UserService : IUserService
{
    private readonly IQueryExecutor _queryExecutor;

    public UserService(IQueryExecutor queryExecutor)
    {
        _queryExecutor = queryExecutor;
    }
    
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _queryExecutor.GetByIdAsync<User>(id);
    }
    
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return (await _queryExecutor.GetByFieldAsync<User>("username", username)).FirstOrDefault();
    }
}
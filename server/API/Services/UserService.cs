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
    private readonly IDataContextDapper _dapper;

    public UserService(IDataContextDapper dapper)
    {
        _dapper = dapper;
    }
    
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _dapper.GetByIdAsync<User>(id);
    }
    
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return (await _dapper.GetByFieldAsync<User>("username", username)).FirstOrDefault();
    }
}
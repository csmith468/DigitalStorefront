using API.Models.DsfTables;
using API.Setup;

namespace API.Services;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByUsernameAsync(string username);
}

public class UserService(ISharedContainer container) : BaseService(container), IUserService
{
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await Dapper.GetByIdAsync<User>(id);
    }
    
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return (await Dapper.GetByFieldAsync<User>("username", username)).FirstOrDefault();
    }
}
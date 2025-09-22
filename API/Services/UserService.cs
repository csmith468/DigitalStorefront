using API.Models;
using API.Setup;

namespace API.Services;

public interface IUserService
{
    User GetUserById(int id);
    User GetUserByUsername(string username);
}

public class UserService(ISharedContainer container) : BaseService(container), IUserService
{
    public User GetUserById(int id)
    {
        return Dapper.GetById<User>(id);
    }
    
    public User GetUserByUsername(string username)
    {
        return Dapper.GetByField<User>("username", username).FirstOrDefault();
    }
}
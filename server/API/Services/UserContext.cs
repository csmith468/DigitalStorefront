namespace API.Services;

public interface IUserContext
{
    int? UserId { get; }
    bool IsAdmin();
    bool HasRole(string roleName);
    List<string> GetRoles();
}

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public int? UserId
    {
        get
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public bool IsAdmin() => HasRole("Admin");
    
    public bool HasRole(string roleName) => GetRoles().Contains(roleName);

    public List<string> GetRoles()
    {
        return httpContextAccessor.HttpContext?.User
            .FindAll("role").Select(r => r.Value).ToList() ?? [];
    }
}
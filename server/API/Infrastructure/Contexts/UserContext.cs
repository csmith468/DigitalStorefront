using System.Security.Claims;
using API.Models.Constants;

namespace API.Infrastructure.Contexts;

public interface IUserContext
{
    int? UserId { get; }
    bool IsAdmin();
    bool HasRole(string roleName);
    List<string> GetRoles();
}

/// <summary>
/// Allows services to get user ID for validation
/// Example: In demo mode, users with certain roles can only add up to 3 products
/// </summary>
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

    public bool IsAdmin() => HasRole(RoleNames.Admin);
    
    public bool HasRole(string roleName) => GetRoles().Contains(roleName);

    public List<string> GetRoles()
    {
        return httpContextAccessor.HttpContext?.User
            .FindAll(ClaimTypes.Role).Select(r => r.Value).ToList() ?? [];
    }
}
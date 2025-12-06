namespace API.Infrastructure.Contexts;

/// <summary>
/// HTTP-based implementation to get user ID from HTTP request's JWT claims
/// Meant to allow Dapper to fill audit fields (CreatedBy/UpdatedBy) without HTTP context
/// </summary>
public class HttpAuditContext : IAuditContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpAuditContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
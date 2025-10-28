using API.Services.Contexts;

namespace API.Services;

/// <summary>
/// Abstraction so DatabaseManager can run Dapper queries without needing HTTP context
/// </summary>
public class SystemAuditContext : IAuditContext
{
    public int? UserId => null;
}
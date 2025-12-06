namespace API.Infrastructure.Contexts;

/// <summary>
/// Abstraction so DatabaseManager can run Dapper queries without needing HTTP context
/// </summary>
public class SystemAuditContext : IAuditContext
{
    public int? UserId => null;
}
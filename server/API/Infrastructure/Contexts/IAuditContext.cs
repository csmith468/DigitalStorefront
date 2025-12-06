namespace API.Infrastructure.Contexts;

/// <summary>
///  Abstraction to get current user ID for audit fields (CreatedBy/UpdatedBy)
///  Allows Dapper to automatically fill audit fields without depending on HTTP context
///  Allows migrations to run Dapper queries without needing HTTP context
/// </summary>
public interface IAuditContext
{
    int? UserId { get; }
}
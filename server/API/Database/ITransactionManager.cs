using System.Data;

namespace API.Database;

/// <summary>
/// Transaction management with automatic commit/rollback
/// Support transaction propagation (nested transactions use the existing one)
/// </summary>
public interface ITransactionManager
{
    Task<T> WithTransactionAsync<T>(Func<Task<T>> func);
    Task WithTransactionAsync(Func<Task> func);
    IDbTransaction? Transaction { get; }
}
using System.Data;

namespace API.Database;

public interface ITransactionManager
{
    Task<T> WithTransactionAsync<T>(Func<Task<T>> func);
    Task WithTransactionAsync(Func<Task> func);
    IDbTransaction? Transaction { get; }
}
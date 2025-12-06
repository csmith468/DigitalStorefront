using API.Database;
using API.Models.DboTables;

namespace API.Infrastructure;

public interface IIdempotencyService
{
    Task<IdempotencyKey?> GetExistingAsync(string clientKey, string endpoint, CancellationToken ct = default);
    Task StoreAsync(IdempotencyKey key, CancellationToken ct = default);
}

public class IdempotencyService : IIdempotencyService
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICommandExecutor _commandExecutor;

    public IdempotencyService(IQueryExecutor queryExecutor, ICommandExecutor commandExecutor)
    {
        _queryExecutor = queryExecutor;
        _commandExecutor = commandExecutor;
    }

    public async Task<IdempotencyKey?> GetExistingAsync(string clientKey, string endpoint, CancellationToken ct = default)
    {
        var results = await _queryExecutor.QueryAsync<IdempotencyKey>(
            """
                SELECT * FROM dbo.idempotencyKey
                WHERE clientKey = @clientKey 
                AND endpoint = @endpoint 
                AND expiresAt > GETUTCDATE()
            """,
            new { clientKey, endpoint }, ct);

        return results.FirstOrDefault();
    }

    public async Task StoreAsync(IdempotencyKey key, CancellationToken ct = default)
    {
        await _commandExecutor.InsertAsync(key, ct);
    }
}
using API.Models;

namespace API.Utils;

public static class Guard
{
    // If null, return failure not found result, otherwise return success result
    // Usage: Any get dapper method
    public static async Task<Result<T>> AgainstNull<T>(Func<Task<T?>> operation, string? failureMessage = null,
        int statusCode = 404) where T : class
    {
        var result = await operation();
        return result == null
            ? Result<T>.Failure(failureMessage ?? $"{typeof(T).Name} not found", statusCode)
            : Result<T>.Success(result);
    }

    // If validation is false, return failure result, otherwise success result 
    // Usage: Dapper.ExistsAsync, Dapper.ExistsByFieldAsync
    public static async Task<Result<bool>> Against(Func<Task<bool>> validation, string? failureMessage = null,
        int statusCode = 404)
    {
        return !await validation() 
            ? Result<bool>.Failure(failureMessage ?? "Not found", statusCode)
            : Result<bool>.Success(true);
    }

    // If list is empty (should use on final step of service since converting to list here)
    // Only use this if it should error if results are empty
    public static async Task<Result<List<T>>> AgainstEmpty<T>(Func<Task<List<T>>> operation, string? emptyMessage = null) where T : class
    {
        var result = (await operation()).ToList();
        
        if (emptyMessage != null && result.Count == 0)
            return  Result<List<T>>.Failure(emptyMessage, 404);
        return Result<List<T>>.Success(result);
    }
}
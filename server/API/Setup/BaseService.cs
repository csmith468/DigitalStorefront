using System.Net;
using API.Database;
using API.Models;
using API.Utils;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Setup;

public class BaseService(ISharedContainer container)
{
    protected readonly IDataContextDapper Dapper = container.Dapper;
    protected readonly IConfiguration Config = container.Config;
    protected readonly IMapper Mapper = container.Mapper;
    protected T DepInj<T>() where T : class => container.DepInj<T>()!;

    protected async Task<Result<T>> GetOrFailAsync<T>(int id, string? message = null) where T : class
    {
        var entity = await Dapper.GetByIdAsync<T>(id);
        return entity == null
            ? Result<T>.Failure(message ?? $"{typeof(T).Name}Id = {id} not found", HttpStatusCode.NotFound)
            : Result<T>.Success(entity);
    }

    protected async Task<Result<bool>> ValidateExistsAsync<T>(int id, string? message = null) where T : class
    {
        return await Dapper.ExistsAsync<T>(id)
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(message ?? $"{typeof(T).Name}Id = {id} not found", HttpStatusCode.NotFound);
    }

    protected async Task<Result<bool>> ValidateExistsByFieldAsync<T>(string fieldName, object value, string? message = null) where T : class
    {
        return await Dapper.ExistsByFieldAsync<T>(fieldName, value)
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(message ?? $"{typeof(T).Name}.{fieldName} = {value} not found", HttpStatusCode.NotFound);
    }
}
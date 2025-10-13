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

    // Shorthand of guard for getting by ID
    protected Task<Result<T>> GetOrFailAsync<T>(int id, string? message = null) where T : class
        => Guard.AgainstNull(
            () => Dapper.GetByIdAsync<T>(id),
            message ?? $"{typeof(T).Name}Id = {id.ToString()} not found"
        );
    
    // Shorthand of guard for checking if exists by ID
    protected Task<Result<bool>> ValidateExistsAsync<T>(int id, string? message = null) where T : class
        => Guard.Against(
            () => Dapper.ExistsAsync<T>(id),
            message ?? $"{typeof(T).Name}Id = {id.ToString()} not found"
        );
    
    // Shorthand of guard for checking if exists by field
    protected Task<Result<bool>> ValidateExistsByFieldAsync<T>(string fieldName, object value, string? message = null) where T : class
        => Guard.Against(
            () => Dapper.ExistsByFieldAsync<T>(fieldName, value),
            message ?? $"{typeof(T).Name}.{fieldName} = {value} not found"
        );
}
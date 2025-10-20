using API.Database;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Setup;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class BaseController(ISharedContainer container) : ControllerBase
{
    protected readonly IDataContextDapper Dapper = container.Dapper;
    protected readonly IConfiguration Config = container.Config;
    protected readonly IMapper Mapper = container.Mapper;
    protected T DepInj<T>() where T : class => container.DepInj<T>()!;

    protected int? UserId
    {
        get
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
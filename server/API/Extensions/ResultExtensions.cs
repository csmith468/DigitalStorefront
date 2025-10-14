using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public static class ResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this Result<T> result)    {
        if (result.IsSuccess) 
            return new OkObjectResult(result.Data);
        return result.StatusCode switch
        {
            400 => new BadRequestObjectResult(result.Data),
            401 => new UnauthorizedObjectResult(result.Data),
            404 => new NotFoundObjectResult(result.Data),
            _ => new ObjectResult(result.Error) { StatusCode = result.StatusCode }
        };
    }
}
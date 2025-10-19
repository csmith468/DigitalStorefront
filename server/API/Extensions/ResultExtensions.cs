using API.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public static class ResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this Result<T> result)    {
        if (result.IsSuccess)
        {
            return result.StatusCode switch
            {
                201 => new ObjectResult(result.Data) { StatusCode = 201 },
                _ => new ObjectResult(result.Data)
            };
        }
        return result.StatusCode switch
        {
            400 => new BadRequestObjectResult(result.Error),
            401 => new UnauthorizedObjectResult(result.Error),
            404 => new NotFoundObjectResult(result.Error),
            _ => new ObjectResult(result.Error) { StatusCode = result.StatusCode }
        };
    }
}
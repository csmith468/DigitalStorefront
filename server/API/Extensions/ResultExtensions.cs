using System.Net;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public static class ResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this Result<T> result)
    {
        return result.IsSuccess 
            ? new ObjectResult(result.Data) { StatusCode = (int)result.StatusCode }
            : new ObjectResult(result.Error) { StatusCode = (int)result.StatusCode };
    }
    
    public static Result<TNew> ToFailure<T, TNew>(this Result<T> result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot convert successful result.");
        return Result<TNew>.Failure(result.Error!, result.StatusCode);
    }
}
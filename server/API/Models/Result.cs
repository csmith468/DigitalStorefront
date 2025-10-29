using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace API.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T Data { get; }
    public string? Error { get; }
    public HttpStatusCode StatusCode { get; }

    private Result(bool isSuccess, T data, string? error, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result<T> Success(T data, HttpStatusCode statusCode = HttpStatusCode.OK) => new(true, data, null, statusCode);
    public static Result<T> Failure(ErrorMessage error) => new(false, default!, error.Message, error.StatusCode);
    
    // only to be used by ResultExtensions for ToFailure
    internal static Result<T> Failure(string error, HttpStatusCode statusCode) =>
        new(false, default!, error, statusCode);
}
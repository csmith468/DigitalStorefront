using Microsoft.AspNetCore.Mvc;

namespace API.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T Data { get; }
    public string? Error { get; }
    public int StatusCode { get; }

    private Result(bool isSuccess, T data, string? error, int statusCode = 200)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result<T> Success(T data, int statusCode = 200) => new(true, data, null, statusCode);
    public static Result<T> Failure(string error, int statusCode = 404) => new(false, default!, error, statusCode);
}
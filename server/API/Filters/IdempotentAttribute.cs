using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Models.DboTables;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class IdempotentAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var keyHeader))
        {
            context.Result = new BadRequestObjectResult(new { error = "Idempotency-Key header is required" });
            return;
        }

        var clientKey = keyHeader.ToString();
        var endpoint = context.HttpContext.Request.Path.Value ?? "";
        var requestHash = await ComputeRequestHashAsync(context.HttpContext.Request);

        var idempotencyService = context.HttpContext.RequestServices.GetRequiredService<IIdempotencyService>();

        var existing = await idempotencyService.GetExistingAsync(clientKey, endpoint);
        if (existing != null)
        {
            if (existing.RequestHash != requestHash)
            {
                context.Result = new ConflictObjectResult(new
                    { error = "Idempotency key already used with different request data" });
                return;
            }

            context.HttpContext.Response.Headers["Idempotent-Replayed"] = "true";
            context.Result = new ContentResult
            {
                StatusCode = existing.StatusCode,
                Content = existing.Response,
                ContentType = "application/json"
            };
            return;
        }

        var resultContext = await next(); // execute
        
        // Store result for future duplicate requests
        if (resultContext.Result is ObjectResult objectResult)
        {
            var response = JsonSerializer.Serialize(objectResult.Value);
            var statusCode = objectResult.StatusCode ?? 200;

            var key = new IdempotencyKey
            {
                ClientKey = clientKey,
                Endpoint = endpoint,
                RequestHash = requestHash,
                StatusCode = statusCode,
                Response = response,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            try
            {
                await idempotencyService.StoreAsync(key);
            }
            catch (Exception ex)
            {
                // Unique constraint violation - not an issue
            }
        }
    }

    private static async Task<string> ComputeRequestHashAsync(HttpRequest request)
    {
        request.EnableBuffering(); // allow reading request body multiple times
        request.Body.Position = 0; // in case something already started reading it

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        request.Body.Position = 0; // reset position so model binding can read after this

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(body)); // unique fingerprint of content
        return Convert.ToBase64String(hashBytes);
    }
}
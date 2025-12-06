using API.Extensions;
using API.Services.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

// NOTE: Interceptor to check access before it even gets to endpoint execution
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireProductAccessAttribute: Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ActionArguments.TryGetValue("productId", out var idStr) || idStr is not int productId)
        {
            context.Result = new BadRequestObjectResult(new { error = "productId route parameter is required" });
            return;
        }

        var authService = context.HttpContext.RequestServices.GetRequiredService<IProductAuthorizationService>();
        var result = await authService.CanUserManageProductAsync(productId);

        if (!result.IsSuccess)
        {
            context.Result = result.ToActionResult().Result;
            return;
        }

        await next(); // execute controller action
    }
}
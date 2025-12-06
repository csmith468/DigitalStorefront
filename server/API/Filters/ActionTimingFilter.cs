using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

public class ActionTimingFilter : IAsyncActionFilter
{
    private readonly ILogger<ActionTimingFilter> _logger;

    public ActionTimingFilter(ILogger<ActionTimingFilter> logger)
    {
        _logger = logger;
    }

    // NOTE: logs how long it takes for action to succeed/fail
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var actionName = context.ActionDescriptor.DisplayName; // like API.Controllers.ProductsController.GetProductAsync
        var arguments = context.ActionArguments; // like { "productId": 1 }
        _logger.LogDebug("Executing {Action} with arguments {@Arguments}", actionName, arguments);

        var stopwatch = Stopwatch.StartNew();
        var resultContext = await next(); // actually execute endpoint
        stopwatch.Stop();

        var duration = stopwatch.ElapsedMilliseconds;
        if (resultContext is { Exception: not null, ExceptionHandled: false })
            _logger.LogWarning("Action {Action} failed after {Duration}ms", actionName, duration);
        else
            _logger.LogInformation("Action {Action} succeeded after {Duration}ms", actionName, duration);
    }
}
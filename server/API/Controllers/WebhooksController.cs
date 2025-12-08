using API.Configuration;
using API.Services.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly IPaymentWebhookService _webhookService;
    private readonly IOptions<StripeOptions> _stripeOptions;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(IPaymentWebhookService webhookService, IOptions<StripeOptions> stripeOptions,
        ILogger<WebhooksController> logger)
    {
        _webhookService = webhookService;
        _stripeOptions = stripeOptions;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhookAsync()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeSignature = Request.Headers["Stripe-Signature"];
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _stripeOptions.Value.WebhookSecret);
            _logger.LogInformation("Received Stripe webhook: {EventType} {EventId}", stripeEvent.Type, stripeEvent.Id);

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    if (stripeEvent.Data.Object is PaymentIntent successIntent)
                        await _webhookService.HandlePaymentSucceededAsync(successIntent);
                    break;

                case "payment_intent.payment_failed":
                    if (stripeEvent.Data.Object is PaymentIntent failedIntent)
                        await _webhookService.HandlePaymentFailedAsync(failedIntent);
                    break;

                default:
                    _logger.LogInformation("Unhandled event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed");
            return BadRequest("Invalid signature");
        }
    }
}
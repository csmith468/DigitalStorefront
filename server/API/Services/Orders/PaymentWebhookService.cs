using API.Database;
using API.Models.DsfTables;
using Stripe;

namespace API.Services.Orders;

public interface IPaymentWebhookService
{
    Task HandlePaymentSucceededAsync(PaymentIntent paymentIntent);
    Task HandlePaymentFailedAsync(PaymentIntent paymentIntent);
}

public class PaymentWebhookService : IPaymentWebhookService
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ILogger<PaymentWebhookService> _logger;

    public PaymentWebhookService(IQueryExecutor queryExecutor, ICommandExecutor commandExecutor,
        ILogger<PaymentWebhookService> logger)
    {
        _queryExecutor = queryExecutor;
        _commandExecutor = commandExecutor;
        _logger = logger;
    }

    public Task HandlePaymentSucceededAsync(PaymentIntent paymentIntent) =>
        UpdateOrderStatusAsync(paymentIntent, "Completed");
    

    public Task HandlePaymentFailedAsync(PaymentIntent paymentIntent) =>
        UpdateOrderStatusAsync(paymentIntent, "Failed");

    private async Task UpdateOrderStatusAsync(PaymentIntent paymentIntent, string newStatus)
    {
        var order = (await _queryExecutor.GetByFieldAsync<Order>("StripePaymentIntentId", paymentIntent.Id))
            .FirstOrDefault();

        if (order == null)
        {
            _logger.LogWarning("Received webhook for unknown PaymentIntent {Id}", paymentIntent.Id);
            return;
        }

        if (order.Status == newStatus) return; // Idempotency

        order.Status = newStatus;
        if (order.Status == "Completed") 
            order.PaymentCompletedAt = DateTime.UtcNow;
        
        await _commandExecutor.UpdateAsync(order, null);
        
        _logger.LogInformation("Order {OrderId} marked as {Status}", order.OrderId, newStatus);
    }
}
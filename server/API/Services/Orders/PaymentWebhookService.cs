using API.Database;
using API.Models.DsfTables;
using API.Models.Dtos;
using API.Services.Notifications;
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
    private readonly IEmailDeliveryService _emailDeliveryService;
    private readonly ILogger<PaymentWebhookService> _logger;

    public PaymentWebhookService(IQueryExecutor queryExecutor, ICommandExecutor commandExecutor,
        IEmailDeliveryService emailDeliveryService, ILogger<PaymentWebhookService> logger)
    {
        _queryExecutor = queryExecutor;
        _commandExecutor = commandExecutor;
        _emailDeliveryService = emailDeliveryService;
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
        
        await _commandExecutor.UpdateAsync(order, order.UpdatedAt);
        
        _logger.LogInformation("Order {OrderId} marked as {Status}", order.OrderId, newStatus);

        if (order.Status == "Completed")
            await SendOrderConfirmationEmailAsync(order);
    }

    private async Task SendOrderConfirmationEmailAsync(Order order)
    {
        if (order.Email == null) return;

        var orderItems = await _queryExecutor.GetByFieldAsync<OrderItem>("orderId", order.OrderId);
        var itemList = string.Join(", ", orderItems.Select(i => i.ProductName));

        await _emailDeliveryService.CreateAndSendAsync(new CreateNotificationRequest
        {
            Order = order,
            Subject = "Thanks for Testing!",
            Body = $"Test Order #{order.OrderId} (${order.TotalCents / 100m:F2}) - {itemList}"
        });
    }
}
using API.Configuration;
using API.Database;
using API.Extensions;
using API.Infrastructure.Contexts;
using API.Models;
using API.Models.Constants;
using API.Models.DboTables;
using API.Models.DsfTables;
using API.Models.Dtos;
using Microsoft.Extensions.Options;

namespace API.Services.Orders;

public interface ICheckoutService
{
    Task<Result<PaymentIntentResponse>> ExecuteCheckoutWorkflowAsync(CreatePaymentIntentRequest request, CancellationToken ct);
}

public class CheckoutService : ICheckoutService
{
    private readonly IUserContext _userContext;
    private readonly IQueryExecutor _queryExecutor;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ITransactionManager _transactionManager;
    private readonly IOptions<StripeOptions> _stripeOptions;
    private readonly ILogger<CheckoutService> _logger;
    
    // When I decided to build this project I didn't know I would build a payment processor
    // I would add a setting for the conversion that admins can edit but not worth implementing it at this point
    // and I'd implement premium memberships 
    private const double CoinToUsdInCents = 0.1; 
    
    public CheckoutService(IUserContext userContext, IQueryExecutor queryExecutor, ICommandExecutor commandExecutor,
        ITransactionManager transactionManager, IOptions<StripeOptions> stripeOptions, ILogger<CheckoutService> logger)
    {
        _userContext = userContext;
        _queryExecutor = queryExecutor;
        _commandExecutor = commandExecutor;
        _transactionManager = transactionManager;
        _stripeOptions = stripeOptions;
        _logger = logger;
    }

    public async Task<Result<PaymentIntentResponse>> ExecuteCheckoutWorkflowAsync(CreatePaymentIntentRequest request, CancellationToken ct)
    {
        var orderIdAndTotal = await CreateOrderAsync(request, ct);
        if (!orderIdAndTotal.IsSuccess)
            return orderIdAndTotal.ToFailure<OrderIdAndTotal, PaymentIntentResponse>();
        
        return await CreatePaymentIntentWithStripeAsync(orderIdAndTotal.Data, ct);
    }

    private async Task<Result<OrderIdAndTotal>> CreateOrderAsync(CreatePaymentIntentRequest request, CancellationToken ct)
    {
        var product = await _queryExecutor.GetByIdAsync<Product>(request.ProductId, ct);
        if (product == null)
            return Result<OrderIdAndTotal>.Failure(ErrorMessages.Product.NotFound(request.ProductId));
        
        var unitPrice = product.PriceTypeId == PriceTypes.Usd
            ? (int)(product.Price * 100)
            : (int)(product.Price * (decimal)CoinToUsdInCents);
        var total = unitPrice * request.Quantity;
        
        var orderId = await _transactionManager.WithTransactionAsync(async () =>
        {
            var id = await _commandExecutor.InsertAsync(new Order { UserId = _userContext.UserId, TotalCents = total }, ct);
            await _commandExecutor.InsertAsync(new OrderItem
            {
                OrderId = id,
                ProductId = product.ProductId,
                ProductName = product.Name,
                UnitPriceCents = unitPrice,
                Quantity = request.Quantity
            }, ct);
            return id;
        }, ct);
        
        return Result<OrderIdAndTotal>.Success(new OrderIdAndTotal(orderId, total));
    }

    private async Task<Result<PaymentIntentResponse>> CreatePaymentIntentWithStripeAsync(OrderIdAndTotal orderIdAndTotal, CancellationToken ct)
    {
        var paymentIntentService = new Stripe.PaymentIntentService();
        var paymentIntent = await paymentIntentService.CreateAsync(new Stripe.PaymentIntentCreateOptions
        {
            Amount = orderIdAndTotal.Total,
            Currency = "usd",
            Metadata = new Dictionary<string, string> { { "order_id", orderIdAndTotal.OrderId.ToString() } }
        }, cancellationToken: ct);

        var order = await _queryExecutor.GetByIdAsync<Order>(orderIdAndTotal.OrderId, ct);
        order!.StripePaymentIntentId = paymentIntent.Id;
        order.Status = "Processing";
        await _commandExecutor.UpdateAsync(order, order.UpdatedAt, ct);

        return Result<PaymentIntentResponse>.Success(new PaymentIntentResponse
        {
            ClientSecret = paymentIntent.ClientSecret,
            OrderId = orderIdAndTotal.OrderId
        });
    }
    
    private record OrderIdAndTotal(int OrderId, int Total);
}














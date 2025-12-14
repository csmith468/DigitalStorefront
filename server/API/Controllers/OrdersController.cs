using API.Extensions;
using API.Filters;
using API.Models.Dtos;
using API.Services.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ICheckoutService _checkoutService;
    
    public OrdersController(IOrderService orderService, ICheckoutService checkoutService)
    {
        _orderService = orderService;
        _checkoutService = checkoutService;
    }

    [EnableRateLimiting("anonymous")]
    [HttpGet] 
    public async Task<ActionResult<PaginatedResponse<OrderDetailDto>>> GetOrdersAsync(
        [FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        return (await _orderService.GetOrdersAsync(pagination, ct)).ToActionResult();
    }

    [Idempotent]
    [EnableRateLimiting("expensive")]
    [HttpPost("payment-intent")]
    public async Task<ActionResult<PaymentIntentResponse>> CreateOrderAsync(CreatePaymentIntentRequest request, CancellationToken ct)
    {
        return (await _checkoutService.ExecuteCheckoutWorkflowAsync(request, ct)).ToActionResult();
    }
}
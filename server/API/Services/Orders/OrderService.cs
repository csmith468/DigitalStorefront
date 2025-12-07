using API.Database;
using API.Models;
using API.Models.DsfTables;
using API.Models.Dtos;
using AutoMapper;

namespace API.Services.Orders;

public interface IOrderService
{
    Task<Result<PaginatedResponse<OrderDetailDto>>> GetOrdersAsync(PaginationParams pagination, CancellationToken ct);
}

public class OrderService : IOrderService
{
    private readonly IQueryExecutor _queryExecutor;
    private readonly IMapper _mapper;
    
    public OrderService(IQueryExecutor queryExecutor, IMapper mapper)
    {
        _queryExecutor = queryExecutor;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResponse<OrderDetailDto>>> GetOrdersAsync(PaginationParams pagination, CancellationToken ct)
    {
        var orders = await _queryExecutor.GetPaginatedWithSqlAsync<Order>(
            baseQuery: "SELECT * FROM dsf.[order]", paginationParams: pagination, ct: ct);
        var orderDtos = orders.items.Select(o => _mapper.Map<OrderDetailDto>(o)).ToList();

        var orderItems = await _queryExecutor.GetWhereInAsync<OrderItem>("orderId", orderDtos.Select(o => o.OrderId).ToList(), ct);
        orderDtos.ForEach(o => o.OrderItems = orderItems.Where(oi => oi.OrderId == o.OrderId).Select(oi => _mapper.Map<OrderItemDto>(oi)).ToList());

        return Result<PaginatedResponse<OrderDetailDto>>.Success(new PaginatedResponse<OrderDetailDto>
        {
            Items = orderDtos,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = orders.totalCount
        });
    }
}
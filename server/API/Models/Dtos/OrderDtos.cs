namespace API.Models.Dtos;
public class OrderDetailDto
{
    public int OrderId { get; set; }
    public int? UserId { get; set; }
    public string Status { get; set; } = "Pending";
    public int TotalCents { get; set; }
    public DateTime? PaymentCompletedAt { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = [];
}

public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int UnitPriceCents { get; set; }
    public int Quantity { get; set; }
    public int TotalCents => UnitPriceCents * Quantity; // would this also be cmoputed if using automapper?
}
using API.Database;

namespace API.Models.DsfTables;

[DbTable("dsf.orderItem")]
public class OrderItem
{
    [DbPrimaryKey] public int OrderItemId { get; set; }
    [DbColumn] public int OrderId { get; set; }
    [DbColumn] public int ProductId { get; set; }
    [DbColumn] public string ProductName { get; set; } = "";
    [DbColumn] public int UnitPriceCents { get; set; }
    [DbColumn] public int Quantity { get; set; }
    [DbColumn] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int TotalCents => UnitPriceCents * Quantity;
}
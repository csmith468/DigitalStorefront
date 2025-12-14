using API.Database;

namespace API.Models.DsfTables;

[DbTable("dsf.[order]")]
public class Order
{
    [DbPrimaryKey] public int OrderId { get; set; }
    [DbColumn] public int? UserId { get; set; } // guest mode for demo
    [DbColumn] public string? StripeSessionId { get; set; }
    [DbColumn] public string? StripePaymentIntentId { get; set; }
    [DbColumn] public string Status { get; set; } = "Pending";
    [DbColumn] public int TotalCents { get; set; }
    [DbColumn] public string? Email { get; set; }
    [DbColumn] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [DbColumn] public DateTime? UpdatedAt { get; set; }
    [DbColumn] public DateTime? PaymentCompletedAt { get; set; }
}
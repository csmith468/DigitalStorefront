using API.Database;

namespace API.Models.DsfTables;

[DbTable("dsf.emailDelivery")]
public class EmailDelivery
{
    [DbPrimaryKey] public int EmailDeliveryId { get; set; }
    [DbColumn] public int OrderId { get; set; }
    [DbColumn] public string Email { get; set; } = "";
    [DbColumn] public string Subject { get; set; } = "";
    [DbColumn] public string Body { get; set; } = "";
    [DbColumn] public string Status { get; set; } = "Pending";
    [DbColumn] public int AttemptCount { get; set; }
    [DbColumn] public DateTime? LastAttemptAt { get; set; }
    [DbColumn] public DateTime? SentAt { get; set; }
    [DbColumn] public string? FailedReason { get; set; }
    [DbColumn] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [DbColumn] public DateTime? UpdatedAt { get; set; }
}
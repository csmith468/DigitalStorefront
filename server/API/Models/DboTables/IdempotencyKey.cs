using API.Database;

namespace API.Models.DboTables;

[DbTable("dbo.idempotencyKey")]
public class IdempotencyKey
{
    [DbPrimaryKey] public int IdempotencyKeyId { get; set; }
    [DbColumn] public string ClientKey { get; set; } = "";
    [DbColumn] public string Endpoint { get; set; } = "";
    [DbColumn] public string RequestHash { get; set; } = "";
    [DbColumn] public int StatusCode { get; set; }
    [DbColumn] public string Response { get; set; } = "";
    [DbColumn] public DateTime CreatedAt { get; set; }
    [DbColumn] public DateTime ExpiresAt { get; set; }
}
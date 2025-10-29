using API.Database;

namespace API.Models.DboTables;

[DbTable("dbo.tag")]
public class Tag
{
    [DbPrimaryKey] public int TagId { get; set; }

    [DbColumn] public string Name { get; set; } = "";
    
    [DbColumn] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
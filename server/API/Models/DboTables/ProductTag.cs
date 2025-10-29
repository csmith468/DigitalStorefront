using API.Database;

namespace API.Models.DboTables;

[DbTable("dbo.productTag")]
public class ProductTag
{
    [DbPrimaryKey] public int ProductTagId { get; set; }
    
    [DbColumn] public int ProductId { get; set; }
    
    [DbColumn] public int TagId { get; set; }
}
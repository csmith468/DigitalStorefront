using API.Database;

namespace Api.Models.DsfTables;

[DbTable("dsf.role")]
public class Role
{
    [DbPrimaryKey] public int RoleId { get; set; }
    [DbColumn] public string RoleName { get; set; } = "";
    [DbColumn] public string? Description { get; set; }
    [DbColumn] public DateTime CreatedAt { get; set; }
}
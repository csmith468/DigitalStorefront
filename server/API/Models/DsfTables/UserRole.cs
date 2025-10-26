using API.Database;

namespace API.Models.DsfTables;

[DbTable("dsf.userRole")]
public class UserRole
{
    [DbPrimaryKey] public int UserRoleId { get; set; }
    [DbColumn] public int UserId { get; set; }
    [DbColumn] public int RoleId { get; set; }
    [DbColumn] public DateTime CreatedAt { get; set; }
}
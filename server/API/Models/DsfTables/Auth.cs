using API.Database;

namespace API.Models.DsfTables;

[DbTable("dsf.[auth]")]
public class Auth
{
    [DbPrimaryKey] public int AuthId { get; set; }
    [DbColumn] public int UserId { get; set; }
    [DbColumn] public byte[] PasswordSalt { get; set; } = [];
    [DbColumn] public byte[] PasswordHash { get; set; } = [];
}
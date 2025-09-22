using API.Database;

namespace API.Models;

[DbTable("dsf.[user]")]
public class User
{
    [DbPrimaryKey] public int UserId { get; set; }
    [DbColumn] public string Username { get; set; }
    [DbColumn] public string FirstName { get; set; }
    [DbColumn] public string LastName { get; set; }
    [DbColumn] public string Email { get; set; }
    [DbColumn] public bool IsActive { get; set; } = true;
}

[DbTable("dsf.[auth]")]
public class Auth
{
    [DbPrimaryKey] public int AuthId { get; set; }
    [DbColumn] public int UserId { get; set; }
    [DbColumn] public byte[] PasswordSalt { get; set; }
    [DbColumn] public byte[] PasswordHash { get; set; }
}
namespace API.Models.Dtos;

public class AuthResponseDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public List<string> Roles { get; set; } = [];
    public string Token { get; set; } = "";
}

public class UserRegisterDto
{
    public string Username { get; set; } = "";
    public string? FirstName { get; set; } = null;
    public string? LastName { get; set; } = null;
    public string? Email { get; set; } = null;
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
}

public class UserLoginDto
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
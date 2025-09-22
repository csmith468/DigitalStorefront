using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Database;
using API.Models;
using API.Services;
using API.Setup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("auth")]
public class AuthController(ISharedContainer container) : BaseController(container)
{
    private IUserService UserService => DepInj<IUserService>();
    
    [AllowAnonymous]
    [HttpPost("register")]
    public int Register(UserRegisterDto userDto)
    {
        ValidateRegistration(userDto);
        var userId = 0;

        Dapper.WithTransaction(() =>
        {
            userId = Dapper.Insert(new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
            });
            CreateAuth(userId, userDto.Password);
        });
        
        return userId;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public Dictionary<string, string> Login(UserLoginDto userDto)
    {
        var user = UserService.GetUserByUsername(userDto.Username);
        if (user == null) throw new Exception("User not found.");

        var userAuth = Dapper.GetByField<Auth>("userId", user.UserId).FirstOrDefault();
        if (userAuth == null) throw new Exception("User not found.");
        
        var passwordHash = GetPasswordHash(userDto.Password, userAuth.PasswordSalt);
        if (passwordHash.Where((t, index) => t != userAuth.PasswordHash[index]).Any())
            throw new Exception("Incorrect password.");

        return new Dictionary<string, string> { { "token", CreateToken(user.UserId) } };
    }

    [HttpPost("refresh-token")]
    public Dictionary<string, string> RefreshToken()
    {
        var userIdStr = User.FindFirst("userId")?.Value;
        if (userIdStr == null) throw new Exception("User not found.");
        var userId = int.Parse(userIdStr);
        
        if (UserService.GetUserById(userId) == null) 
            throw new Exception("User not found.");

        return new Dictionary<string, string>
        {
            { "token", CreateToken(userId) }
        };
    }

    private void ValidateRegistration(UserRegisterDto user)
    {
        if (user.Password != user.ConfirmPassword)
            throw new ValidationException("Passwords do not match.");
        if (Dapper.ExistsByField<User>("email", user.Email))
            throw new ValidationException("Email already exists.");
        if (Dapper.ExistsByField<User>("username", user.Username))
            throw new ValidationException("Username already exists.");
    }

    private byte[] GetPasswordHash(string password, byte[] passwordSalt)
    {
        var passwordSaltKey = Config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);
        return KeyDerivation.Pbkdf2(
            password: password, 
            salt: Encoding.ASCII.GetBytes(passwordSaltKey),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8
        );
    }

    private void CreateAuth(int userId, string password)
    {
        var passwordSalt = new byte[128 / 8];
        using var rng = RandomNumberGenerator.Create();
        rng.GetNonZeroBytes(passwordSalt);

        var passwordHash = GetPasswordHash(password, passwordSalt);

        Dapper.Insert(new Auth
        {
            UserId = userId,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        });
    }

    private string CreateToken(int userId)
    {
        var claims = new[] { new Claim("userId", userId.ToString()) };

        var configTokenKey = Config.GetSection("AppSettings:TokenKey").Value;
        if (configTokenKey is null) throw new Exception("Cannot create token.");
        var tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configTokenKey));
        
        var credentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha256);
        var descriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = credentials,
            Expires = DateTime.Now.AddDays(1),
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(descriptor);
        return tokenHandler.WriteToken(token);
    }

    public class UserRegisterDto
    {
        public string Username { get; set; }
        public string FirstName { get; set; } = null;
        public string LastName { get; set; } = null;
        public string Email { get; set; } = null;
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class UserLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
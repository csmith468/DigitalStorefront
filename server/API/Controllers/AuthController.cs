using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using API.Models.Dsf;
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
    public async Task<ActionResult<AuthResponseDto>> Register(UserRegisterDto userDto)
    {
        await ValidateRegistrationAsync(userDto);
        var userId = 0;

        await Dapper.WithTransactionAsync(async () =>
        {
            userId = await Dapper.InsertAsync(new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
            });
            await CreateAuthAsync(userId, userDto.Password);
        });
        
        var token = CreateToken(userId);
        var response = new AuthResponseDto
        {
            UserId = userId,
            Username = userDto.Username,
            Token = token
        };
        
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(UserLoginDto userDto)
    {
        var user = await UserService.GetUserByUsernameAsync(userDto.Username);
        if (user == null) return NotFound("User not found.");

        var userAuth = (await Dapper.GetByFieldAsync<Auth>("userId", user.UserId)).FirstOrDefault();
        if (userAuth == null) return NotFound("User not found.");
        
        var passwordHash = GetPasswordHash(userDto.Password, userAuth.PasswordSalt);
        if (passwordHash.Where((t, index) => t != userAuth.PasswordHash[index]).Any())
            return BadRequest("Incorrect password.");

        var response = new AuthResponseDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Token = CreateToken(user.UserId)
        };
        
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken()
    {
        var userIdStr = User.FindFirst("userId")?.Value;
        if (userIdStr == null) return Unauthorized("Invalid token.");
        var userId = int.Parse(userIdStr);
        
        var user = await UserService.GetUserByIdAsync(userId);
        if (user == null) return NotFound("User not found.");

        var response = new AuthResponseDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Token = CreateToken(user.UserId)
        };
        
        return Ok(response);
    }

    private async Task ValidateRegistrationAsync(UserRegisterDto user)
    {
        if (user.Password != user.ConfirmPassword)
            throw new ValidationException("Passwords do not match.");
        if (user.Email is not null && await Dapper.ExistsByFieldAsync<User>("email", user.Email))
            throw new ValidationException("Email already exists.");
        if (await Dapper.ExistsByFieldAsync<User>("username", user.Username))
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

    private async Task CreateAuthAsync(int userId, string password)
    {
        var passwordSalt = new byte[128 / 8];
        using var rng = RandomNumberGenerator.Create();
        rng.GetNonZeroBytes(passwordSalt);

        var passwordHash = GetPasswordHash(password, passwordSalt);

        await Dapper.InsertAsync(new Auth
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
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = credentials,
            Expires = DateTime.Now.AddDays(1),
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(descriptor);
        return tokenHandler.WriteToken(token);
    }

    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
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
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Setup;
using Microsoft.IdentityModel.Tokens;

namespace API.Utils;

public class TokenGenerator(IServiceProvider serviceProvider)
{
    private IConfiguration Config => serviceProvider.GetService<IConfiguration>()!;
    
    public string GenerateToken(int userId)
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
}
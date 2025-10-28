using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Utils;

public class TokenGenerator
{
    private readonly SecurityOptions _securityOptions;

    public TokenGenerator(IOptions<SecurityOptions> securityOptions)
    {
        _securityOptions = securityOptions.Value;
    }
    
    public string GenerateToken(int userId, List<string> roles)
    {
        var claims = new List<Claim> { new ("userId", userId.ToString()) };
        claims.AddRange(roles.Select(role => new Claim("role", role)));

        var tokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityOptions.TokenKey));
        
        var credentials = new SigningCredentials(tokenKey, SecurityAlgorithms.HmacSha256);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = credentials,
            Expires = DateTime.UtcNow.AddDays(1),
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(descriptor);
        return tokenHandler.WriteToken(token);
    }
}
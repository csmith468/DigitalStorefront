using System.Security.Cryptography;
using System.Text;
using API.Configuration;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;

namespace API.Utils;

public class PasswordHasher
{
    private readonly SecurityOptions _securityOptions;

    public PasswordHasher(IOptions<SecurityOptions> securityOptions)
    {
        _securityOptions = securityOptions.Value;
    }

    public (byte[] salt, byte[] hash) HashPassword(string password)
    {
        var passwordSalt = new byte[128 / 8];
        using var rng = RandomNumberGenerator.Create();
        rng.GetNonZeroBytes(passwordSalt);
        
        var passwordHash = GetPasswordHash(password, passwordSalt);
        return (passwordSalt, passwordHash);
    }

    public bool VerifyPassword(string password, byte[] salt, byte[] hash)
    {
        var computedHash = GetPasswordHash(password, salt);
        return CryptographicOperations.FixedTimeEquals(computedHash, hash);
    }
    
    private byte[] GetPasswordHash(string password, byte[] passwordSalt)
    {
        var passwordSaltKey = _securityOptions.PasswordKey + Convert.ToBase64String(passwordSalt);
        return KeyDerivation.Pbkdf2(
            password: password, 
            salt: Encoding.ASCII.GetBytes(passwordSaltKey),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8
        );
    }
}
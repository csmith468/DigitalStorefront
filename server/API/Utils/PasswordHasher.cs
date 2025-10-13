using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace API.Utils;

public class PasswordHasher(IServiceProvider serviceProvider)
{
    private IConfiguration Config => serviceProvider.GetService<IConfiguration>()!;

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
        return computedHash.SequenceEqual(hash);
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
}
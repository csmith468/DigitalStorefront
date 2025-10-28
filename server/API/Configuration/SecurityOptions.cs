using System.ComponentModel.DataAnnotations;

namespace API.Configuration;

public class SecurityOptions
{
    public const string SectionName = "AppSettings";

    [Required(ErrorMessage = "TokenKey is required for JWT authentication")]
    [MinLength(32, ErrorMessage = "TokenKey must be at least 32 characters to be secure")]
    public string TokenKey { get; set; } = "";
    
    [Required(ErrorMessage = "PasswordKey is required for JWT authentication")]
    [MinLength(32, ErrorMessage = "PasswordKey must be at least 32 characters to be secure")]
    public string PasswordKey { get; set; } = "";
}
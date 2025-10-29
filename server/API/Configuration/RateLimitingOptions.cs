using System.ComponentModel.DataAnnotations;

namespace API.Configuration;

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    [Range(1, 10000, ErrorMessage = "PermitLimit must be between 1 and 10000.")]
    public int PermitLimit { get; set; } = 100;
    
    [Range(1, 60, ErrorMessage = "WindowMinutes must be between 1 and 60.")]
    public int WindowMinutes { get; set; } = 60;
}
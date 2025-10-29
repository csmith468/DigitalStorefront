using System.ComponentModel.DataAnnotations;

namespace API.Configuration;

public class CachingOptions
{
    public const string SectionName = "Caching";
    
    [Range(1, 365, ErrorMessage = "StaticDataExpirationDays must be between 1 and 365.")]
    public int StaticDataExpirationDays { get; set; } = 1;
}
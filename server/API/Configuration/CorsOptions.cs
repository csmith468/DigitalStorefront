using System.ComponentModel.DataAnnotations;

namespace API.Configuration;

public class CorsOptions
{
    public const string SectionName = "Cors";

    [Required]
    [MinLength(1, ErrorMessage = "At least one dev origin is required.")]
    public List<string> DevOrigins { get; set; } = [];
    
    [Required]
    [MinLength(1, ErrorMessage = "At least one prod origin is required.")]
    public List<string> ProdOrigins { get; set; } = [];
}
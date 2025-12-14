using System.ComponentModel.DataAnnotations;

namespace API.Configuration;

public class SendGridOptions
{
    public const string SectionName = "SendGrid";

    [Required(ErrorMessage = "SendGrid API key is required")]
    public string ApiKey { get; set; } = "";

    [Required(ErrorMessage = "SendGrid from email is required")]
    public string FromEmail { get; set; } = "";

    public string FromName { get; set; } = "Digital Storefront";
}
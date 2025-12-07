using System.ComponentModel.DataAnnotations;

namespace API.Configuration;

public class StripeOptions
{
    public const string SectionName = "Stripe";

    [Required(ErrorMessage = "Stripe secret key is required")]
    public string SecretKey { get; set; } = "";
    
    [Required(ErrorMessage = "Stripe webhook secret is required")]
    public string WebhookSecret { get; set; } = "";
}
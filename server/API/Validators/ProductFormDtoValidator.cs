using API.Models.Constants;
using API.Models.Dtos;
using FluentValidation;

namespace API.Validators;

public class ProductFormDtoValidator : AbstractValidator<ProductFormDto>
{
    public ProductFormDtoValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(255).WithMessage("Product name must not exceed 255 characters");

        RuleFor(p => p.Slug)
            .NotEmpty().WithMessage("Product slug is required")
            .MaximumLength(100).WithMessage("Product slug must not exceed 100 characters")
            .Matches("^[a-z0-9-]+$").WithMessage("Slug may only contain lowercase letters, numbers, and hyphens");
        
        RuleFor(p => p.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
            .When(p => !string.IsNullOrEmpty(p.Description));
        
        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThanOrEqualTo(99999).WithMessage("Price must be less than 99999");
        
        RuleFor(p => p.PremiumPrice)
            .GreaterThan(0).WithMessage("Premium price must be greater than 0")
            .LessThanOrEqualTo(99999).WithMessage("Premium price must be less than 99999")
            .LessThanOrEqualTo(p => p.Price)
            .WithMessage("Premium price cannot exceed regular price");

        RuleFor(p => p.PriceTypeId)
            .Must(BeValidPriceType).WithMessage("Invalid price type");
        
        RuleFor(p => p.SubcategoryIds)
            .NotEmpty().WithMessage("At least one subcategory must be selected")
            .Must(subcategoryIds => subcategoryIds.Count <= 10).WithMessage("Cannot have more than 10 subcategories");
    }

    private bool BeValidPriceType(int priceTypeId)
    {
        return PriceTypes.All.Any(pt => pt.PriceTypeId == priceTypeId);
    }
}
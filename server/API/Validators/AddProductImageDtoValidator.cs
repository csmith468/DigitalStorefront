using API.Models.Dtos;
using FluentValidation;

namespace API.Validators;

public class AddProductImageDtoValidator : AbstractValidator<AddProductImageDto>
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    
    public AddProductImageDtoValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("Image file is required")
            .Must(file => file.Length > 0).WithMessage("Image file cannot be empty")
            .Must(file => file.Length <= MaxFileSize)
            .WithMessage($"Image file size cannot exceed {MaxFileSize / 1024 / 1024}MB")
            .Must(HaveValidExtension)
            .WithMessage($"Only image files are allowed ({string.Join(", ",  _allowedExtensions)})");
    }
    
    private bool HaveValidExtension(IFormFile? file)
    {
        if (file == null) return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }
}
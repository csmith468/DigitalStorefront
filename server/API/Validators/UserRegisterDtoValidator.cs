using API.Models.Dtos;
using FluentValidation;

namespace API.Validators;

public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
{
    public UserRegisterDtoValidator()
    {
        RuleFor(u => u.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain letters, numbers, underscores, and hyphens");
        
        RuleFor(u => u.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(u => !string.IsNullOrEmpty(u.Email));
        
        RuleFor(u => u.FirstName)
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
            .When(u => !string.IsNullOrEmpty(u.FirstName));
        
        RuleFor(u => u.LastName)
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
            .When(u => !string.IsNullOrEmpty(u.LastName));
        
        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters");
        
        RuleFor(u => u.ConfirmPassword)
            .Equal(u => u.Password).WithMessage("Passwords do not match");
    }
}
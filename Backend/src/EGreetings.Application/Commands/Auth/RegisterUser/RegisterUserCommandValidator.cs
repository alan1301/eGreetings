using EGreetings.Domain.ValueObjects;
using EGreetings.Shared.Constants;
using FluentValidation;

namespace EGreetings.Application.Commands.Auth.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

        // BR-01
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(256)
            .Must(e => Email.TryCreate(e, out _)).WithMessage("Invalid email format.");

        // BR-01: Password complexity
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(BusinessConstants.PasswordMinLength)
                .WithMessage($"Password must be at least {BusinessConstants.PasswordMinLength} characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least 1 uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least 1 lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least 1 digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least 1 special character.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}

using EGreetings.Shared.Constants;
using FluentValidation;

namespace EGreetings.Application.Commands.Auth.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống")
            .MaximumLength(100).WithMessage("Họ tên tối đa 100 ký tự");

        // BR-01
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Email không đúng định dạng")
            .MaximumLength(256);

        // BR-01: Password complexity
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống")
            .MinimumLength(BusinessConstants.PasswordMinLength)
                .WithMessage($"Mật khẩu phải có ít nhất {BusinessConstants.PasswordMinLength} ký tự")
            .Matches("[A-Z]").WithMessage("Mật khẩu phải có ít nhất 1 ký tự hoa")
            .Matches("[a-z]").WithMessage("Mật khẩu phải có ít nhất 1 ký tự thường")
            .Matches("[0-9]").WithMessage("Mật khẩu phải có ít nhất 1 chữ số")
            .Matches("[^a-zA-Z0-9]").WithMessage("Mật khẩu phải có ít nhất 1 ký tự đặc biệt");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Mật khẩu xác nhận không khớp");
    }
}

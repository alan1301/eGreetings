using MediatR;

namespace EGreetings.Application.Features.Users.Commands.ChangePassword;

/// <summary>UC19 A1 - Đổi mật khẩu</summary>
public record ChangePasswordCommand(
    int UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
) : IRequest<bool>;

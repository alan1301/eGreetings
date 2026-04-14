using MediatR;

namespace EGreetings.Application.Features.Auth.Commands.ResetPassword;

/// <summary>UC22 - Đặt lại mật khẩu (bước 2)</summary>
public record ResetPasswordCommand(string Token, string NewPassword, string ConfirmPassword) : IRequest<bool>;

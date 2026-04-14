using MediatR;

namespace EGreetings.Application.Features.Auth.Commands.ForgotPassword;

/// <summary>UC22 - Quên mật khẩu & Đặt lại mật khẩu (bước 1)</summary>
public record ForgotPasswordCommand(string Email) : IRequest<bool>;

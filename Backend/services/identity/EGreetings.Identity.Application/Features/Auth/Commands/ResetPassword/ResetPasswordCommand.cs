using MediatR;

namespace EGreetings.Identity.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommand : IRequest<Unit>
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

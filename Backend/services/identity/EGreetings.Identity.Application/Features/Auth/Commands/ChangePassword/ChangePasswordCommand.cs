using MediatR;

namespace EGreetings.Identity.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommand : IRequest<Unit>
{
    public int UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

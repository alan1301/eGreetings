using MediatR;

namespace EGreetings.Identity.Application.Features.Auth.Commands.Register;

public class RegisterCommand : IRequest<int>
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

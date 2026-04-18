using MediatR;

namespace EGreetings.Identity.Application.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommand : IRequest<Unit>
{
    public string Token { get; set; } = string.Empty;
}

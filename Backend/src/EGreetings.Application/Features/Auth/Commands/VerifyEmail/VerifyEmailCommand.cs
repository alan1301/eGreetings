using MediatR;

namespace EGreetings.Application.Features.Auth.Commands.VerifyEmail;

/// <summary>UC01 - Xác thực email qua token</summary>
public record VerifyEmailCommand(string Token) : IRequest<bool>;

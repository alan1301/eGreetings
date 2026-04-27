using MediatR;

namespace EGreetings.Application.Commands.Auth.RegisterUser;

public record RegisterUserCommand(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword
) : IRequest<RegisterUserResult>;

public record RegisterUserResult(
    Guid UserId,
    string Message
);

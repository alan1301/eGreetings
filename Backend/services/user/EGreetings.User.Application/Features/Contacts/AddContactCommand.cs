using MediatR;

namespace EGreetings.User.Application.Features.Contacts;

public record AddContactCommand(
    int UserId,
    string Name,
    string Email,
    string? Phone,
    DateTime? Birthday,
    string? Note,
    string? Group
) : IRequest<int>;

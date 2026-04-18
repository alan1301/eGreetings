using MediatR;

namespace EGreetings.User.Application.Features.Contacts;

public record DeleteContactCommand(
    int UserId,
    int ContactId
) : IRequest<bool>;

using MediatR;

namespace EGreetings.User.Application.Features.Profile;

public record UpdateProfileCommand(
    int UserId,
    string? FullName,
    string? Phone,
    string? AvatarUrl
) : IRequest<bool>;

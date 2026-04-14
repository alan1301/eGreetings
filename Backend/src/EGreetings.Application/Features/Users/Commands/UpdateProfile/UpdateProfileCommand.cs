using MediatR;

namespace EGreetings.Application.Features.Users.Commands.UpdateProfile;

/// <summary>UC19 - Cập nhật hồ sơ cá nhân</summary>
public record UpdateProfileCommand(
    int UserId,
    string? FullName,
    string? Phone,
    string? AvatarUrl
) : IRequest<bool>;

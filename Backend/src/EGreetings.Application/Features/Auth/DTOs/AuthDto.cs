namespace EGreetings.Application.Features.Auth.DTOs;

public record AuthResponseDto(
    int UserId,
    string FullName,
    string Email,
    string Role,
    string Token,
    DateTime TokenExpiry
);

public record UserProfileDto(
    int Id,
    string FullName,
    string Email,
    string? Phone,
    string? AvatarUrl,
    string Role,
    string Status,
    DateTime CreatedAt
);

using EGreetings.Application.Features.Users.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Users.Queries.GetProfile;

/// <summary>UC19 - Xem hồ sơ cá nhân</summary>
public record GetProfileQuery(int UserId) : IRequest<UserProfileDto>;

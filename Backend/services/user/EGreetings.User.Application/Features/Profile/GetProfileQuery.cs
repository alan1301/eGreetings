using MediatR;
using EGreetings.User.Application.DTOs;

namespace EGreetings.User.Application.Features.Profile;

public record GetProfileQuery(int UserId) : IRequest<UserProfileDto?>;

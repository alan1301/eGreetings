using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.User.Application.Common.Interfaces;
using EGreetings.User.Application.DTOs;

namespace EGreetings.User.Application.Features.Profile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserProfileDto?>
{
    private readonly IUserDbContext _context;

    public GetProfileQueryHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDto?> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile == null)
        {
            return null;
        }

        return new UserProfileDto(
            profile.Id,
            profile.UserId,
            profile.Email,
            profile.FullName,
            profile.Phone,
            profile.AvatarUrl,
            profile.CreatedAt
        );
    }
}

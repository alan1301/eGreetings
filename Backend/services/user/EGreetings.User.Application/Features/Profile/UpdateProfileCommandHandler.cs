using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.User.Application.Common.Interfaces;

namespace EGreetings.User.Application.Features.Profile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, bool>
{
    private readonly IUserDbContext _context;

    public UpdateProfileCommandHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile == null)
        {
            throw new InvalidOperationException("User profile not found");
        }

        if (!string.IsNullOrEmpty(request.FullName))
        {
            profile.FullName = request.FullName;
        }

        if (!string.IsNullOrEmpty(request.Phone))
        {
            profile.Phone = request.Phone;
        }

        if (!string.IsNullOrEmpty(request.AvatarUrl))
        {
            profile.AvatarUrl = request.AvatarUrl;
        }

        _context.UserProfiles.Update(profile);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

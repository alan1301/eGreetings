using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Entities;
using MediatR;

namespace EGreetings.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public UpdateProfileCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<User>();
        var user = await repo.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Người dùng không tồn tại.");

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.Phone != null) user.Phone = request.Phone;
        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        repo.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}

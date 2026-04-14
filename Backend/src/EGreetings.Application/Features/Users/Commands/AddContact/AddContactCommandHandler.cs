using EGreetings.Application.Common.Interfaces;
using EGreetings.Domain.Entities;
using MediatR;

namespace EGreetings.Application.Features.Users.Commands.AddContact;

public class AddContactCommandHandler : IRequestHandler<AddContactCommand, int>
{
    private readonly IUnitOfWork _uow;

    public AddContactCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<int> Handle(AddContactCommand request, CancellationToken cancellationToken)
    {
        // BR-20: Mỗi tài khoản chỉ được lưu tối đa 200 liên hệ
        var currentCount = await _uow.Repository<Contact>()
            .CountAsync(c => c.UserId == request.UserId, cancellationToken);
        if (currentCount >= 200)
            throw new InvalidOperationException("BR-20: Bạn đã đạt giới hạn 200 liên hệ. Xóa bớt để tiếp tục.");

        var contact = new Contact
        {
            UserId = request.UserId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Birthday = request.Birthday,
            Note = request.Note,
            Group = request.Group
        };

        _uow.Repository<Contact>().Add(contact);
        await _uow.SaveChangesAsync(cancellationToken);

        return contact.Id;
    }
}

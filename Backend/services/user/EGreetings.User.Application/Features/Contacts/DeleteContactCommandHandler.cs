using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.User.Application.Common.Interfaces;

namespace EGreetings.User.Application.Features.Contacts;

public class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand, bool>
{
    private readonly IUserDbContext _context;

    public DeleteContactCommandHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == request.ContactId && c.UserId == request.UserId, cancellationToken);

        if (contact == null)
        {
            throw new InvalidOperationException("Contact not found");
        }

        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

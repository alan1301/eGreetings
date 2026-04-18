using MediatR;
using EGreetings.User.Application.Common.Interfaces;
using EGreetings.User.Domain.Entities;

namespace EGreetings.User.Application.Features.Contacts;

public class AddContactCommandHandler : IRequestHandler<AddContactCommand, int>
{
    private readonly IUserDbContext _context;

    public AddContactCommandHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(AddContactCommand request, CancellationToken cancellationToken)
    {
        if (!IsValidEmail(request.Email))
        {
            throw new InvalidOperationException("Invalid email format");
        }

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

        await _context.Contacts.AddAsync(contact, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return contact.Id.GetHashCode();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

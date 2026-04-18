using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.User.Application.Common.Interfaces;
using EGreetings.User.Application.DTOs;

namespace EGreetings.User.Application.Features.Contacts;

public class GetContactsQueryHandler : IRequestHandler<GetContactsQuery, List<ContactDto>>
{
    private readonly IUserDbContext _context;

    public GetContactsQueryHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContactDto>> Handle(GetContactsQuery request, CancellationToken cancellationToken)
    {
        var contacts = await _context.Contacts
            .Where(c => c.UserId == request.UserId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ContactDto(
                c.Id,
                c.Name,
                c.Email,
                c.Phone,
                c.Birthday,
                c.Note,
                c.Group,
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return contacts;
    }
}

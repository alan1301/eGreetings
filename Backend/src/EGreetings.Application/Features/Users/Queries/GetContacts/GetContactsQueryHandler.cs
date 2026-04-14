using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Users.DTOs;
using EGreetings.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Users.Queries.GetContacts;

public class GetContactsQueryHandler : IRequestHandler<GetContactsQuery, IReadOnlyList<ContactDto>>
{
    private readonly IRepository<Contact> _contactRepo;

    public GetContactsQueryHandler(IRepository<Contact> contactRepo)
    {
        _contactRepo = contactRepo;
    }

    public async Task<IReadOnlyList<ContactDto>> Handle(GetContactsQuery request, CancellationToken cancellationToken)
    {
        var contacts = await _contactRepo.Query()
            .Where(c => c.UserId == request.UserId)
            .OrderBy(c => c.Name)
            .Select(c => new ContactDto(c.Id, c.UserId, c.Name, c.Email, c.Phone, c.Birthday, c.Note, c.Group, c.CreatedAt))
            .ToListAsync(cancellationToken);

        return contacts;
    }
}

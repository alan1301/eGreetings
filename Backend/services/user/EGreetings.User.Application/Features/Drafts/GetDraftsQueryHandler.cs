using MediatR;
using Microsoft.EntityFrameworkCore;
using EGreetings.User.Application.Common.Interfaces;
using EGreetings.User.Application.DTOs;

namespace EGreetings.User.Application.Features.Drafts;

public class GetDraftsQueryHandler : IRequestHandler<GetDraftsQuery, List<DraftDto>>
{
    private readonly IUserDbContext _context;

    public GetDraftsQueryHandler(IUserDbContext context)
    {
        _context = context;
    }

    public async Task<List<DraftDto>> Handle(GetDraftsQuery request, CancellationToken cancellationToken)
    {
        var drafts = await _context.Drafts
            .Where(d => d.UserId == request.UserId)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DraftDto(
                d.Id,
                d.TemplateId,
                d.Title,
                d.CustomHtml,
                d.RecipientEmail,
                d.RecipientName,
                d.SenderMessage,
                d.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return drafts;
    }
}

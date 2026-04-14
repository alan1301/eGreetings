using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Users.DTOs;
using EGreetings.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Users.Queries.GetDrafts;

public class GetDraftsQueryHandler : IRequestHandler<GetDraftsQuery, IReadOnlyList<DraftDto>>
{
    private readonly IRepository<Draft> _draftRepo;

    public GetDraftsQueryHandler(IRepository<Draft> draftRepo)
    {
        _draftRepo = draftRepo;
    }

    public async Task<IReadOnlyList<DraftDto>> Handle(GetDraftsQuery request, CancellationToken cancellationToken)
    {
        var drafts = await _draftRepo.Query()
            .Include(d => d.Template)
            .Where(d => d.UserId == request.UserId)
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .Select(d => new DraftDto(
                d.Id, d.UserId, d.TemplateId,
                d.Template != null ? d.Template.Name : string.Empty,
                d.Title, d.RecipientEmail, d.RecipientName, d.SenderMessage,
                d.CreatedAt, d.UpdatedAt))
            .ToListAsync(cancellationToken);

        return drafts;
    }
}

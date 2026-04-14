using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Admin.DTOs;
using EGreetings.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Queries.GetContentVersions;

public class GetContentVersionsQueryHandler : IRequestHandler<GetContentVersionsQuery, IReadOnlyList<WebContentVersionDto>>
{
    private readonly IRepository<WebContent> _contentRepo;

    public GetContentVersionsQueryHandler(IRepository<WebContent> contentRepo)
    {
        _contentRepo = contentRepo;
    }

    public async Task<IReadOnlyList<WebContentVersionDto>> Handle(GetContentVersionsQuery request, CancellationToken cancellationToken)
    {
        var versions = await _contentRepo.Query()
            .Where(wc => wc.Key == request.Key)
            .OrderByDescending(wc => wc.Version)
            .Take(10)
            .Select(wc => new WebContentVersionDto(
                wc.Id, wc.Key, wc.Title, wc.Version,
                wc.IsActive, wc.UpdatedByUserId, wc.CreatedAt))
            .ToListAsync(cancellationToken);

        return versions;
    }
}

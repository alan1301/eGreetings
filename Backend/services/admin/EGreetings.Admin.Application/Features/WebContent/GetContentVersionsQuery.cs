using EGreetings.Admin.Application.Common.Interfaces;
using EGreetings.Admin.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Admin.Application.Features.WebContent;

public record GetContentVersionsQuery(string Key) : IRequest<List<WebContentVersionDto>>;

public class GetContentVersionsQueryHandler : IRequestHandler<GetContentVersionsQuery, List<WebContentVersionDto>>
{
    private readonly IAdminDbContext _dbContext;

    public GetContentVersionsQueryHandler(IAdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<WebContentVersionDto>> Handle(GetContentVersionsQuery request, CancellationToken cancellationToken)
    {
        var webContent = await _dbContext.WebContents
            .FirstOrDefaultAsync(w => w.Key == request.Key, cancellationToken);

        if (webContent == null)
            return new List<WebContentVersionDto>();

        var versions = await _dbContext.WebContentVersions
            .Where(v => v.WebContentId == webContent.Id)
            .OrderByDescending(v => v.Version)
            .Select(v => new WebContentVersionDto(
                v.Id,
                v.WebContentId,
                v.Title,
                v.Version,
                v.ArchivedAt))
            .ToListAsync(cancellationToken);

        return versions;
    }
}

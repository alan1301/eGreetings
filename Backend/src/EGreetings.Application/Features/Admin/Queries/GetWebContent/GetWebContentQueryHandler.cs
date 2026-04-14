using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Admin.Queries.GetWebContent;

public class GetWebContentQueryHandler : IRequestHandler<GetWebContentQuery, WebContentDto?>
{
    private readonly IApplicationDbContext _context;

    public GetWebContentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WebContentDto?> Handle(GetWebContentQuery request, CancellationToken cancellationToken)
    {
        var content = await _context.WebContents
            .Where(c => c.Key == request.Key && c.IsActive)
            .OrderByDescending(c => c.Version)
            .FirstOrDefaultAsync(cancellationToken);

        if (content == null)
            return null;

        return new WebContentDto(
            content.Id,
            content.Key,
            content.Title,
            content.Content,
            content.ImageUrl,
            content.Version,
            content.IsActive,
            content.UpdatedByUserId,
            content.CreatedAt
        );
    }
}

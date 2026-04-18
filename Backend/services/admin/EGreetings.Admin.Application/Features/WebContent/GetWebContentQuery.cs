using EGreetings.Admin.Application.Common.Interfaces;
using EGreetings.Admin.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Admin.Application.Features.WebContent;

public record GetWebContentQuery(string Key) : IRequest<WebContentDto?>;

public class GetWebContentQueryHandler : IRequestHandler<GetWebContentQuery, WebContentDto?>
{
    private readonly IAdminDbContext _dbContext;

    public GetWebContentQueryHandler(IAdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WebContentDto?> Handle(GetWebContentQuery request, CancellationToken cancellationToken)
    {
        var content = await _dbContext.WebContents
            .FirstOrDefaultAsync(w => w.Key == request.Key, cancellationToken);

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
            content.CreatedAt);
    }
}

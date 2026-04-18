using EGreetings.Admin.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EGreetings.Admin.Application.Features.WebContent;

public record UpdateWebContentCommand(
    string Key,
    string Title,
    string Content,
    string? ImageUrl,
    int? UpdatedByUserId) : IRequest<int>;

public class UpdateWebContentCommandHandler : IRequestHandler<UpdateWebContentCommand, int>
{
    private readonly IAdminDbContext _dbContext;
    private readonly ILogger<UpdateWebContentCommandHandler> _logger;

    public UpdateWebContentCommandHandler(IAdminDbContext dbContext, ILogger<UpdateWebContentCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> Handle(UpdateWebContentCommand request, CancellationToken cancellationToken)
    {
        var webContent = await _dbContext.WebContents
            .FirstOrDefaultAsync(w => w.Key == request.Key, cancellationToken);

        if (webContent == null)
        {
            _logger.LogWarning("WebContent with key {Key} not found", request.Key);
            return 0;
        }

        // Archive current version
        var archivedVersion = new Domain.Entities.WebContentVersion
        {
            WebContentId = webContent.Id,
            Title = webContent.Title,
            Content = webContent.Content,
            ImageUrl = webContent.ImageUrl,
            Version = webContent.Version,
            CreatedByUserId = request.UpdatedByUserId,
            ArchivedAt = DateTime.UtcNow
        };

        _dbContext.WebContentVersions.Add(archivedVersion);

        // Update current version
        webContent.Title = request.Title;
        webContent.Content = request.Content;
        webContent.ImageUrl = request.ImageUrl;
        webContent.Version++;
        webContent.UpdatedByUserId = request.UpdatedByUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "WebContent updated: Key={Key}, Version={Version}, UpdatedBy={UpdatedBy}",
            request.Key, webContent.Version, request.UpdatedByUserId);

        return webContent.Version;
    }
}

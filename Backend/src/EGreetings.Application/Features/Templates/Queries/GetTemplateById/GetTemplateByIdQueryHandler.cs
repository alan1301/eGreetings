using EGreetings.Application.Common.Interfaces;
using EGreetings.Application.Features.Templates.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Application.Features.Templates.Queries.GetTemplateById;

public class GetTemplateByIdQueryHandler : IRequestHandler<GetTemplateByIdQuery, TemplateDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTemplateByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TemplateDetailDto> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _context.GreetingTemplates
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Mẫu thiệp không tồn tại.");

        // Only allow access if template is active or user is admin
        if (!template.IsActive && !_currentUser.IsAdmin)
            throw new UnauthorizedAccessException("Mẫu thiệp này không có sẵn.");

        return new TemplateDetailDto(
            template.Id,
            template.CategoryId,
            template.Category.Name,
            template.Name,
            template.Description,
            template.ThumbnailUrl,
            template.HtmlContent,
            template.CssStyle,
            template.IsFree,
            template.IsActive,
            template.UsageCount,
            template.CreatedAt
        );
    }
}

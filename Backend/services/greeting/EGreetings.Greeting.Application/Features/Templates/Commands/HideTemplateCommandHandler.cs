using EGreetings.Greeting.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Application.Features.Templates.Commands;

public class HideTemplateCommandHandler : IRequestHandler<HideTemplateCommand, bool>
{
    private readonly IGreetingDbContext _context;

    public HideTemplateCommandHandler(IGreetingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(HideTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.GreetingTemplates
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Template with id {request.Id} not found");

        template.IsActive = !request.Hide;
        template.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

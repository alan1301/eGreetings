using EGreetings.Greeting.Application.Common.Interfaces;
using EGreetings.Greeting.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Application.Features.Admin;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IGreetingDbContext _context;

    public UpdateCategoryCommandHandler(IGreetingDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with id {request.Id} not found");

        if (!string.IsNullOrWhiteSpace(request.Name))
            category.Name = request.Name;

        if (request.Description != null)
            category.Description = request.Description;

        if (request.IconUrl != null)
            category.IconUrl = request.IconUrl;

        if (request.IsActive.HasValue)
            category.IsActive = request.IsActive.Value;

        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var templateCount = await _context.GreetingTemplates
            .CountAsync(t => t.CategoryId == category.Id && t.IsActive, cancellationToken);

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.IconUrl,
            category.IsActive,
            templateCount,
            category.CreatedAt
        );
    }
}

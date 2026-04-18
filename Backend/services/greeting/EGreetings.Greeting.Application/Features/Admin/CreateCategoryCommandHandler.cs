using EGreetings.Greeting.Application.Common.Interfaces;
using EGreetings.Greeting.Application.DTOs;
using EGreetings.Greeting.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EGreetings.Greeting.Application.Features.Admin;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IGreetingDbContext _context;

    public CreateCategoryCommandHandler(IGreetingDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var maxOrder = await _context.Categories
            .MaxAsync(c => (int?)c.DisplayOrder, cancellationToken) ?? 0;

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            IconUrl = request.IconUrl,
            IsActive = true,
            DisplayOrder = maxOrder + 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.IconUrl,
            category.IsActive,
            0,
            category.CreatedAt
        );
    }
}

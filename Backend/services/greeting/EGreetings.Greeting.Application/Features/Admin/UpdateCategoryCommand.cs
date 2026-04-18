using EGreetings.Greeting.Application.DTOs;
using MediatR;

namespace EGreetings.Greeting.Application.Features.Admin;

public record UpdateCategoryCommand(
    int Id,
    string? Name,
    string? Description,
    string? IconUrl,
    bool? IsActive
) : IRequest<CategoryDto>;

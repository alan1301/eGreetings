using EGreetings.Greeting.Application.DTOs;
using MediatR;

namespace EGreetings.Greeting.Application.Features.Admin;

public record CreateCategoryCommand(
    string Name,
    string? Description,
    string? IconUrl
) : IRequest<CategoryDto>;

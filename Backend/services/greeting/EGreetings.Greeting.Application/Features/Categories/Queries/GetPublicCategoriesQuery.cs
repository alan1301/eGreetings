using EGreetings.Greeting.Application.DTOs;
using MediatR;

namespace EGreetings.Greeting.Application.Features.Categories.Queries;

public record GetPublicCategoriesQuery : IRequest<List<CategoryDto>>;

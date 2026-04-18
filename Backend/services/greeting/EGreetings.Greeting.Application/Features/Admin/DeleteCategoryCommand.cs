using MediatR;

namespace EGreetings.Greeting.Application.Features.Admin;

public record DeleteCategoryCommand(int Id) : IRequest<bool>;

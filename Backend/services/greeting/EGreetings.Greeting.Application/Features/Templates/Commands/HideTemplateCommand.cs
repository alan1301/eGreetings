using MediatR;

namespace EGreetings.Greeting.Application.Features.Templates.Commands;

public record HideTemplateCommand(int Id, bool Hide) : IRequest<bool>;

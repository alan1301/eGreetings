using EGreetings.Greeting.Application.DTOs;
using MediatR;

namespace EGreetings.Greeting.Application.Features.Templates.Queries;

public record GetTemplateByIdQuery(int Id) : IRequest<TemplateDetailDto>;

using EGreetings.Application.Features.Templates.DTOs;
using MediatR;

namespace EGreetings.Application.Features.Templates.Queries.GetTemplateById;

/// <summary>UC04 - Xem chi tiết mẫu thiệp</summary>
public record GetTemplateByIdQuery(int Id) : IRequest<TemplateDetailDto>;

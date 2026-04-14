using MediatR;

namespace EGreetings.Application.Features.Templates.Commands.HideTemplate;

/// <summary>UC10 - Ẩn/Hiện mẫu thiệp (Admin only) - BR-18, BR-19</summary>
public record HideTemplateCommand(int Id, bool Hide) : IRequest<bool>;

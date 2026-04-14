using EGreetings.Application.Features.Templates.Commands.CreateTemplate;
using EGreetings.Application.Features.Templates.Commands.HideTemplate;
using EGreetings.Application.Features.Templates.Commands.UpdateTemplate;
using EGreetings.Application.Features.Templates.Queries.GetTemplateById;
using EGreetings.Application.Features.Templates.Queries.GetTemplates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.API.Controllers;

/// <summary>
/// UC03 - Xem danh sách mẫu thiệp | UC04 - Xem chi tiết | UC12 - Quản lý template (Admin)
/// UC23 - View Home Page
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TemplatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>UC03/UC23 - Lấy danh sách mẫu thiệp (Public)</summary>
    [HttpGet]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] int? categoryId,
        [FromQuery] string? keyword,
        [FromQuery] bool? isFree,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetTemplatesQuery(categoryId, keyword, isFree, page, pageSize), ct);
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>UC04 - Xem chi tiết mẫu thiệp</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTemplate(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTemplateByIdQuery(id), ct);
        return Ok(new { Success = true, Data = result });
    }

    /// <summary>UC12 - Tạo mẫu thiệp mới (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetTemplates), new { id = result.Id },
            new { Success = true, Data = result, Message = "Tạo mẫu thiệp thành công." });
    }

    /// <summary>UC10 - Cập nhật mẫu thiệp (Admin only)</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTemplate(int id, [FromBody] UpdateTemplateRequest request, CancellationToken ct)
    {
        var cmd = new UpdateTemplateCommand(id, request.Name, request.Description, request.ThumbnailUrl,
            request.HtmlContent, request.CssStyle, request.IsFree, request.IsActive);
        var result = await _mediator.Send(cmd, ct);
        return Ok(new { Success = true, Data = result, Message = "Cập nhật mẫu thiệp thành công." });
    }

    /// <summary>UC10 - Ẩn/Hiện mẫu thiệp (Admin only)</summary>
    [HttpPatch("{id}/visibility")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetTemplateVisibility(int id, [FromBody] HideTemplateRequest request, CancellationToken ct)
    {
        await _mediator.Send(new HideTemplateCommand(id, request.Hide), ct);
        var message = request.Hide ? "Đã ẩn mẫu thiệp." : "Đã hiện mẫu thiệp.";
        return Ok(new { Success = true, Message = message });
    }
}

public record UpdateTemplateRequest(
    string? Name,
    string? Description,
    string? ThumbnailUrl,
    string? HtmlContent,
    string? CssStyle,
    bool? IsFree,
    bool? IsActive
);

public record HideTemplateRequest(bool Hide);

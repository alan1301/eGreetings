using EGreetings.Greeting.Application.Common.Models;
using EGreetings.Greeting.Application.DTOs;
using EGreetings.Greeting.Application.Features.Templates.Commands;
using EGreetings.Greeting.Application.Features.Templates.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EGreetings.Greeting.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TemplatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TemplateDto>>> GetTemplates(
        [FromQuery] int? categoryId,
        [FromQuery] string? keyword,
        [FromQuery] bool? isFree,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetTemplatesQuery(categoryId, keyword, isFree, page, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TemplateDetailDto>> GetTemplateById(int id)
    {
        var query = new GetTemplateByIdQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TemplateDto>> CreateTemplate([FromBody] CreateTemplateCommand command)
    {
        // Check admin role from header
        if (!IsAdmin())
        {
            return Forbid();
        }

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTemplateById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TemplateDto>> UpdateTemplate(int id, [FromBody] UpdateTemplateRequest request)
    {
        // Check admin role from header
        if (!IsAdmin())
        {
            return Forbid();
        }

        var command = new UpdateTemplateCommand(
            id,
            request.Name,
            request.Description,
            request.ThumbnailUrl,
            request.HtmlContent,
            request.CssStyle,
            request.IsFree,
            request.IsActive
        );
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch("{id}/visibility")]
    public async Task<ActionResult<bool>> HideTemplate(int id, [FromBody] HideTemplateRequest request)
    {
        // Check admin role from header
        if (!IsAdmin())
        {
            return Forbid();
        }

        var command = new HideTemplateCommand(id, request.Hide);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    private bool IsAdmin()
    {
        var userRole = HttpContext.Request.Headers["X-UserRole"].ToString();
        return userRole == "Admin";
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

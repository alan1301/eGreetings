using EGreetings.Admin.Application.DTOs;
using EGreetings.Admin.Application.Features.AuditLog;
using EGreetings.Admin.Application.Features.Dashboard;
using EGreetings.Admin.Application.Features.WebContent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EGreetings.Admin.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IMediator mediator, ILogger<AdminController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    private bool IsAdmin()
    {
        var roleHeader = Request.Headers["X-UserRole"].ToString();
        return roleHeader == "Admin";
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        if (!IsAdmin())
            return Forbid("Only admins can access dashboard");

        _logger.LogInformation("GetDashboard called");

        var query = new GetDashboardQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAuditLogs(
        [FromQuery] string? entityName = null,
        [FromQuery] int? userId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin())
            return Forbid("Only admins can access audit logs");

        _logger.LogInformation("GetAuditLogs called with entityName={EntityName}, userId={UserId}, page={Page}", entityName, userId, page);

        var query = new GetAuditLogsQuery(entityName, userId, from, to, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("audit-logs/export")]
    public async Task<ActionResult> ExportAuditLogs(
        [FromQuery] string? entityName = null,
        [FromQuery] int? userId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin())
            return Forbid("Only admins can export audit logs");

        _logger.LogInformation("ExportAuditLogs called");

        var query = new GetAuditLogsQuery(entityName, userId, from, to, 1, int.MaxValue);
        var result = await _mediator.Send(query, cancellationToken);

        var csv = ConvertToCSV(result.Items);
        var csvBytes = Encoding.UTF8.GetBytes(csv);

        return File(csvBytes, "text/csv", "audit-logs.csv");
    }

    [HttpGet("content/{key}")]
    public async Task<ActionResult<WebContentDto>> GetWebContent(string key, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
            return Forbid("Only admins can access web content");

        _logger.LogInformation("GetWebContent called for key={Key}", key);

        var query = new GetWebContentQuery(key);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Web content with key '{key}' not found");

        return Ok(result);
    }

    [HttpPut("content/{key}")]
    public async Task<ActionResult<int>> UpdateWebContent(
        string key,
        [FromBody] UpdateWebContentRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAdmin())
            return Forbid("Only admins can update web content");

        if (string.IsNullOrEmpty(key))
            return BadRequest("Key is required");

        var userId = Request.Headers["X-UserId"].ToString();
        if (!int.TryParse(userId, out var userGuid))
            return BadRequest("Invalid user ID");

        _logger.LogInformation("UpdateWebContent called for key={Key}", key);

        var command = new UpdateWebContentCommand(key, request.Title, request.Content, request.ImageUrl, userGuid);
        var version = await _mediator.Send(command, cancellationToken);

        return Ok(new { version });
    }

    [HttpGet("content/{key}/versions")]
    public async Task<ActionResult<List<WebContentVersionDto>>> GetContentVersions(
        string key,
        CancellationToken cancellationToken)
    {
        if (!IsAdmin())
            return Forbid("Only admins can view content versions");

        _logger.LogInformation("GetContentVersions called for key={Key}", key);

        var query = new GetContentVersionsQuery(key);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpPost("content/{key}/rollback/{versionId}")]
    public async Task<ActionResult> RollbackWebContent(
        string key,
        int versionId,
        CancellationToken cancellationToken)
    {
        if (!IsAdmin())
            return Forbid("Only admins can rollback content");

        var userId = Request.Headers["X-UserId"].ToString();
        if (!int.TryParse(userId, out var userGuid))
            return BadRequest("Invalid user ID");

        _logger.LogInformation("RollbackWebContent called for key={Key}, versionId={VersionId}", key, versionId);

        var command = new RollbackWebContentCommand(key, versionId, userGuid);
        var success = await _mediator.Send(command, cancellationToken);

        if (!success)
            return BadRequest("Failed to rollback content");

        return Ok("Content rolled back successfully");
    }

    private string ConvertToCSV(List<AuditLogDto> logs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,EventType,EntityName,EntityId,Description,UserId,IpAddress,IsSystemAction,OccurredAt");

        foreach (var log in logs)
        {
            sb.AppendLine($"\"{log.Id}\",\"{log.EventType}\",\"{log.EntityName}\",\"{log.EntityId}\",\"{log.Description}\",\"{log.UserId}\",\"{log.IpAddress}\",\"{log.IsSystemAction}\",\"{log.OccurredAt:yyyy-MM-dd HH:mm:ss}\"");
        }

        return sb.ToString();
    }
}

public record UpdateWebContentRequest(string Title, string Content, string? ImageUrl);

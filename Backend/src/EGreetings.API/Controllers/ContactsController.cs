using EGreetings.Application.Commands.Contacts.CreateContact;
using EGreetings.Application.Interfaces;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EGreetings.API.Controllers;

/// <summary>UC16 – Contact address book management.</summary>
[ApiController]
[Route("api/contacts")]
[Authorize]
[Produces("application/json")]
public class ContactsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _db;

    public ContactsController(IMediator mediator, IAppDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>UC16 – Get all contacts for current user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetContacts(CancellationToken ct)
    {
        var contacts = await _db.Contacts
            .Where(c => c.UserId == CurrentUserId)
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Email,
                Group = c.Group.ToString(),
                c.OccasionDate,
                c.OccasionLabel
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(contacts));
    }

    /// <summary>
    /// Get contacts whose OccasionDate falls within the next 30 days.
    /// Day/month comparison ignores year — annual recurring events.
    /// </summary>
    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcomingEvents(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Fetch contacts that have an occasion date
        var contacts = await _db.Contacts
            .Where(c => c.UserId == CurrentUserId && c.OccasionDate.HasValue)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.OccasionDate,
                c.OccasionLabel
            })
            .ToListAsync(ct);

        // Calculate next occurrence (this year or next) and filter within 30 days
        var upcoming = contacts
            .Select(c =>
            {
                var occ = c.OccasionDate!.Value;
                var thisYear = new DateOnly(today.Year, occ.Month, occ.Day);
                var nextOcc = thisYear >= today ? thisYear : new DateOnly(today.Year + 1, occ.Month, occ.Day);
                var daysLeft = nextOcc.DayNumber - today.DayNumber;
                return new { c.Name, c.OccasionLabel, OccasionDate = nextOcc, DaysLeft = daysLeft };
            })
            .Where(x => x.DaysLeft <= 30)
            .OrderBy(x => x.DaysLeft)
            .ToList();

        return Ok(ApiResponse<object>.Ok(upcoming));
    }

    /// <summary>UC16 – Add contact (BR-20: max 200, BR-21: validation)</summary>
    [HttpPost]
    public async Task<IActionResult> CreateContact([FromBody] CreateContactCommand command, CancellationToken ct)
    {
        var cmd = command with { UserId = CurrentUserId };
        var id = await _mediator.Send(cmd, ct);
        return Created($"/api/contacts/{id}", ApiResponse<object>.Created(new { id }));
    }
}

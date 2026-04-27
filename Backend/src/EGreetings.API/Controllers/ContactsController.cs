using EGreetings.Application.Commands.Contacts.CreateContact;
using EGreetings.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public ContactsController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>UC16 – Add contact (BR-20: max 200, BR-21: validation)</summary>
    [HttpPost]
    public async Task<IActionResult> CreateContact([FromBody] CreateContactCommand command, CancellationToken ct)
    {
        var cmd = command with { UserId = CurrentUserId };
        var id = await _mediator.Send(cmd, ct);
        return Created($"/api/contacts/{id}", ApiResponse<object>.Created(new { id }));
    }
}

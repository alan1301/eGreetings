using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EGreetings.User.Application.DTOs;
using EGreetings.User.Application.Features.Contacts;
using EGreetings.User.Application.Features.Drafts;
using EGreetings.User.Application.Features.Profile;

namespace EGreetings.User.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int GetUserId()
    {
        var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
        return int.TryParse(userIdHeader, out var userId) ? userId : 0;
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto?>> GetProfile()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var profile = await _mediator.Send(new GetProfileQuery(userId));
        return Ok(profile);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var result = await _mediator.Send(new UpdateProfileCommand(userId, command.FullName, command.Phone, command.AvatarUrl));
        return NoContent();
    }

    [HttpGet("contacts")]
    [Authorize]
    public async Task<ActionResult<List<ContactDto>>> GetContacts()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var contacts = await _mediator.Send(new GetContactsQuery(userId));
        return Ok(contacts);
    }

    [HttpPost("contacts")]
    [Authorize]
    public async Task<ActionResult<int>> AddContact([FromBody] AddContactCommand command)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var result = await _mediator.Send(new AddContactCommand(
            userId, command.Name, command.Email, command.Phone, command.Birthday, command.Note, command.Group));
        return CreatedAtAction(nameof(GetContacts), new { }, result);
    }

    [HttpDelete("contacts/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        await _mediator.Send(new DeleteContactCommand(userId, id));
        return NoContent();
    }

    [HttpGet("drafts")]
    [Authorize]
    public async Task<ActionResult<List<DraftDto>>> GetDrafts()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var drafts = await _mediator.Send(new GetDraftsQuery(userId));
        return Ok(drafts);
    }

    [HttpPost("drafts")]
    [Authorize]
    public async Task<ActionResult<int>> SaveDraft([FromBody] SaveDraftCommand command)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var result = await _mediator.Send(new SaveDraftCommand(
            userId, command.TemplateId, command.Title, command.CustomHtml, command.RecipientEmail, command.RecipientName, command.SenderMessage));
        return CreatedAtAction(nameof(GetDrafts), new { }, result);
    }

    [HttpDelete("drafts/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteDraft(int id)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        await _mediator.Send(new DeleteDraftCommand(userId, id));
        return NoContent();
    }
}

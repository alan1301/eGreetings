using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EGreetings.API.Controllers;

/// <summary>
/// Base class for controllers that require authenticated user context.
/// Provides convenient access to claims on the current principal.
/// </summary>
public abstract class AuthorizedControllerBase : ControllerBase
{
    protected Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected string CurrentUserEmail =>
        User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
}

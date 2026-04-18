namespace EGreetings.Shared.Infrastructure.Services;

using Microsoft.AspNetCore.Http;
using EGreetings.Shared.Infrastructure.Interfaces;

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public int? UserId => int.TryParse(accessor.HttpContext?.Request.Headers["X-UserId"], out var id) ? id : null;
    public string? Role => accessor.HttpContext?.Request.Headers["X-UserRole"].FirstOrDefault();
    public bool IsAuthenticated => UserId.HasValue;
    public bool IsAdmin => Role == "Admin";
    public string? Email => accessor.HttpContext?.Request.Headers["X-UserEmail"].FirstOrDefault();
}

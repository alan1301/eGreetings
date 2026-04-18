using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EGreetings.Identity.API.Filters;

public class InternalApiAuthFilter : IAuthorizationFilter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<InternalApiAuthFilter> _logger;

    public InternalApiAuthFilter(IConfiguration configuration, ILogger<InternalApiAuthFilter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var headerSecret = context.HttpContext.Request.Headers["X-Internal-Secret"].ToString();
        if (string.IsNullOrWhiteSpace(headerSecret))
        {
            // Temporary fallback to support existing callers.
            headerSecret = context.HttpContext.Request.Headers["X-Internal-Api-Secret"].ToString();
        }
        var configuredSecret = _configuration["InternalApi:Secret"];

        if (string.IsNullOrEmpty(headerSecret) || headerSecret != configuredSecret)
        {
            _logger.LogWarning("Unauthorized internal API access attempt");
            context.Result = new UnauthorizedResult();
            return;
        }
    }
}

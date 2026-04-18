using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace EGreetings.Shared.Infrastructure.Filters;

public class InternalApiAuthFilter(IConfiguration configuration) : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var expectedSecret = configuration["InternalApi:Secret"];

        if (string.IsNullOrWhiteSpace(expectedSecret))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var providedSecret = context.HttpContext.Request.Headers["X-Internal-Secret"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(providedSecret) || providedSecret != expectedSecret)
        {
            context.Result = new ForbidResult();
        }
    }
}

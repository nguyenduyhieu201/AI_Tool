using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChatGPT.API.Middleware;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // If already authenticated, prefer ClaimsPrincipal
        string? userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                           ?? context.User?.FindFirst("sub")?.Value;

        // Try get JWT from session if available
        if (string.IsNullOrEmpty(userId) && context.Session != null)
        {
            var sessionToken = context.Session.GetString("jwt")
                               ?? context.Session.GetString("Jwt")
                               ?? context.Session.GetString("AccessToken")
                               ?? context.Session.GetString("AuthToken");
            if (!string.IsNullOrWhiteSpace(sessionToken))
            {
                TryExtractUserId(sessionToken!, out userId);
            }
        }

        // Fallback: try parse Bearer token without validation to extract claims (best effort)
        if (string.IsNullOrEmpty(userId))
        {
            var auth = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(auth) && auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = auth.Substring("Bearer ".Length).Trim();
                TryExtractUserId(token, out userId);
            }
        }

        if (!string.IsNullOrEmpty(userId))
        {
            context.Items["UserId"] = userId;
        }

        await _next(context);
    }

    private static void TryExtractUserId(string token, out string? userId)
    {
        userId = null;
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            userId = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;
        }
        catch
        {
            // ignore decoding errors; let downstream handle Unauthorized if needed
        }
    }
}

public static class HttpContextUserExtensions
{
    public static string? GetUserId(this HttpContext context)
        => context.Items.TryGetValue("UserId", out var v) ? v as string : null;
}

public static class UserContextMiddlewareExtensions
{
    public static IApplicationBuilder UseUserContext(this IApplicationBuilder app)
        => app.UseMiddleware<UserContextMiddleware>();
}



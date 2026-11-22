using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace UserManager.Middleware;

public class BlockedUserMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly HashSet<Guid> BlockedUserIds = new()
    {
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Guid.Parse("22222222-2222-2222-2222-222222222222")
    };

    public BlockedUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) &&
            Guid.TryParse(userIdHeader, out var userId) &&
            BlockedUserIds.Contains(userId))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "User Blocked",
                Detail = "Ваш акаунт заблоковано за порушення правил сервісу.",
                Instance = context.Request.Path
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
            return;
        }

        await _next(context);
    }
}

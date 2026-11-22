using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace UserManager.Middleware;

public class LanguageValidationMiddleware
{
    private readonly RequestDelegate _next;

    public LanguageValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Accept-Language", out var acceptLanguage) ||
            string.IsNullOrWhiteSpace(acceptLanguage))
        {
            context.Items["Language"] = "uk-UA";
            await _next(context);
            return;
        }

        var headerValue = acceptLanguage.ToString();
        var tags = headerValue
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part.Split(';')[0].Trim());

        if (tags.Any(tag =>
                string.Equals(tag, "ru", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tag, "ru-RU", StringComparison.OrdinalIgnoreCase)))
        {
            context.Response.StatusCode = StatusCodes.Status406NotAcceptable;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status406NotAcceptable,
                Title = "Unsupported Language",
                Detail = "Обрана мова не підтримується сервісом. Будь ласка, використовуйте uk-UA або en-US.",
                Instance = context.Request.Path
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
            return;
        }

        context.Items["Language"] = headerValue;
        await _next(context);
    }
}

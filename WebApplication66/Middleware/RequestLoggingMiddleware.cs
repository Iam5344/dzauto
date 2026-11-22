using System.Diagnostics;

namespace UserManager.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _logFilePath;

    public RequestLoggingMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        _next = next;
        _logFilePath = Path.Combine(env.ContentRootPath, "logs.txt");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var message = "Success";

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            message = $"Error: {ex.Message}";
            throw;
        }
        finally
        {
            stopwatch.Stop();

            var logEntry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] | " +
                           $"Method: {context.Request.Method} | " +
                           $"Path: {context.Request.Path} | " +
                           $"StatusCode: {context.Response.StatusCode} | " +
                           $"Duration: {stopwatch.ElapsedMilliseconds}ms | " +
                           $"Message: {message}{Environment.NewLine}";

            await File.AppendAllTextAsync(_logFilePath, logEntry);
        }
    }
}

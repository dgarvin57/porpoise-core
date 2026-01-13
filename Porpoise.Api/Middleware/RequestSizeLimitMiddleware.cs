using Microsoft.AspNetCore.Http.Features;

namespace Porpoise.Api.Middleware;

/// <summary>
/// Middleware to handle request size limit exceptions and return user-friendly error messages
/// </summary>
public class RequestSizeLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestSizeLimitMiddleware> _logger;
    private const long MaxSizeBytes = 100 * 1024 * 1024; // 100MB

    public RequestSizeLimitMiddleware(RequestDelegate next, ILogger<RequestSizeLimitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BadHttpRequestException ex) when (ex.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            _logger.LogWarning("Request payload too large: {Message}", ex.Message);
            await HandleRequestTooLarge(context);
        }
        catch (Exception ex) when (ex.InnerException is BadHttpRequestException badHttpEx 
                                   && badHttpEx.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            _logger.LogWarning("Request payload too large: {Message}", ex.Message);
            await HandleRequestTooLarge(context);
        }
    }

    private static async Task HandleRequestTooLarge(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
        context.Response.ContentType = "application/json";
        
        var maxSizeMB = MaxSizeBytes / (1024 * 1024);
        
        await context.Response.WriteAsJsonAsync(new
        {
            error = "File too large",
            message = $"The uploaded file exceeds the maximum allowed size of {maxSizeMB}MB. Please upload a smaller file.",
            statusCode = 413,
            maxSizeBytes = MaxSizeBytes,
            maxSizeMB = maxSizeMB
        });
    }
}

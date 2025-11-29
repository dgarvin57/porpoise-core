using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Services;

namespace Porpoise.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantRepository tenantRepository, TenantContext tenantContext)
    {
        // Skip tenant resolution for CORS preflight requests
        if (context.Request.Method == "OPTIONS")
        {
            await _next(context);
            return;
        }

        // Skip tenant resolution for health checks and swagger
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.Contains("/swagger") || path.Contains("/health"))
        {
            await _next(context);
            return;
        }

        // Extract tenant key from header
        if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantKeyValues))
        {
            _logger.LogWarning("Missing X-Tenant-Id header for request: {Path}", context.Request.Path);
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "X-Tenant-Id header is required" });
            return;
        }

        var tenantKey = tenantKeyValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(tenantKey))
        {
            _logger.LogWarning("Empty X-Tenant-Id header for request: {Path}", context.Request.Path);
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "X-Tenant-Id header cannot be empty" });
            return;
        }

        // Resolve tenant from database
        var tenant = await tenantRepository.GetByKeyAsync(tenantKey);
        if (tenant == null)
        {
            _logger.LogWarning("Invalid tenant key: {TenantKey}", tenantKey);
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = $"Invalid tenant: {tenantKey}" });
            return;
        }

        if (!tenant.IsActive)
        {
            _logger.LogWarning("Inactive tenant attempted access: {TenantKey}", tenantKey);
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Tenant is inactive" });
            return;
        }

        // Set tenant context for the request
        tenantContext.TenantId = tenant.TenantId;
        tenantContext.TenantKey = tenant.TenantKey;
        tenantContext.TenantName = tenant.Name;
        tenantContext.IsActive = tenant.IsActive;

        _logger.LogDebug("Resolved tenant: {TenantKey} (ID: {TenantId})", tenant.TenantKey, tenant.TenantId);

        await _next(context);
    }
}

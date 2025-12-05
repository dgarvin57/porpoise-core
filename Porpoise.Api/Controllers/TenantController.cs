using Microsoft.AspNetCore.Mvc;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;

namespace Porpoise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<TenantController> _logger;

    public TenantController(ITenantRepository tenantRepository, ILogger<TenantController> logger)
    {
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all tenants (admin only)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tenant>>> GetAll()
    {
        var tenants = await _tenantRepository.GetAllAsync();
        return Ok(tenants);
    }

    /// <summary>
    /// Get tenant by ID (admin only)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Tenant>> GetById(string id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant == null)
        {
            return NotFound(new { error = $"Tenant with ID {id} not found" });
        }
        return Ok(tenant);
    }

    /// <summary>
    /// Get tenant by key (admin only)
    /// </summary>
    [HttpGet("by-key/{tenantKey}")]
    public async Task<ActionResult<Tenant>> GetByKey(string tenantKey)
    {
        var tenant = await _tenantRepository.GetByKeyAsync(tenantKey);
        if (tenant == null)
        {
            return NotFound(new { error = $"Tenant with key '{tenantKey}' not found" });
        }
        return Ok(tenant);
    }

    /// <summary>
    /// Create a new tenant (admin only)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Tenant>> Create([FromBody] CreateTenantRequest request)
    {
        // Validate request
        if (string.IsNullOrWhiteSpace(request.TenantKey))
        {
            return BadRequest(new { error = "TenantKey is required" });
        }
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Name is required" });
        }

        // Check if tenant key already exists
        var existing = await _tenantRepository.GetByKeyAsync(request.TenantKey);
        if (existing != null)
        {
            return Conflict(new { error = $"Tenant with key '{request.TenantKey}' already exists" });
        }

        // Create tenant
        var tenant = new Tenant
        {
            TenantKey = request.TenantKey.ToLowerInvariant().Trim(),
            Name = request.Name.Trim(),
            IsActive = true
        };

        var tenantId = await _tenantRepository.AddAsync(tenant);
        tenant.TenantId = tenantId;

        _logger.LogInformation("Created new tenant: {TenantKey} (ID: {TenantId})", tenant.TenantKey, tenant.TenantId);

        return CreatedAtAction(nameof(GetById), new { id = tenant.TenantId }, tenant);
    }

    /// <summary>
    /// Update tenant (admin only)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Tenant>> Update(string id, [FromBody] UpdateTenantRequest request)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant == null)
        {
            return NotFound(new { error = $"Tenant with ID {id} not found" });
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            tenant.Name = request.Name.Trim();
        }
        if (!string.IsNullOrWhiteSpace(request.TenantKey))
        {
            tenant.TenantKey = request.TenantKey.ToLowerInvariant().Trim();
        }
        if (request.IsActive.HasValue)
        {
            tenant.IsActive = request.IsActive.Value;
        }

        await _tenantRepository.UpdateAsync(tenant);

        _logger.LogInformation("Updated tenant: {TenantKey} (ID: {TenantId})", tenant.TenantKey, tenant.TenantId);

        return Ok(tenant);
    }

    /// <summary>
    /// Delete tenant (admin only - use with caution!)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant == null)
        {
            return NotFound(new { error = $"Tenant with ID {id} not found" });
        }

        var deleted = await _tenantRepository.DeleteAsync(id);
        if (!deleted)
        {
            return StatusCode(500, new { error = "Failed to delete tenant" });
        }

        _logger.LogWarning("Deleted tenant: {TenantKey} (ID: {TenantId})", tenant.TenantKey, tenant.TenantId);

        return NoContent();
    }
}

// Request DTOs
public class CreateTenantRequest
{
    public string TenantKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class UpdateTenantRequest
{
    public string? TenantKey { get; set; }
    public string? Name { get; set; }
    public bool? IsActive { get; set; }
}

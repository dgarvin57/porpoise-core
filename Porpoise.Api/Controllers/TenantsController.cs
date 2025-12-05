using Microsoft.AspNetCore.Mvc;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using Porpoise.Api.Models;

namespace Porpoise.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantRepository _tenantRepository;
        private readonly TenantContext _tenantContext;
        private readonly IUnitOfWork _unitOfWork;

        public TenantsController(
            ITenantRepository tenantRepository, 
            TenantContext tenantContext,
            IUnitOfWork unitOfWork)
        {
            _tenantRepository = tenantRepository;
            _tenantContext = tenantContext;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentTenant()
        {
            var tenant = await _tenantRepository.GetByIdAsync(_tenantContext.TenantId);
            if (tenant == null)
            {
                return NotFound();
            }

            var response = new TenantResponse
            {
                TenantId = tenant.TenantId,
                TenantKey = tenant.TenantKey,
                Name = tenant.Name,
                IsActive = tenant.IsActive,
                OrganizationName = tenant.OrganizationName,
                OrganizationLogoBase64 = tenant.OrganizationLogo != null 
                    ? Convert.ToBase64String(tenant.OrganizationLogo) 
                    : null,
                OrganizationTagline = tenant.OrganizationTagline,
                CreatedDate = tenant.CreatedDate,
                ModifiedDate = tenant.ModifiedDate,
                CreatedBy = tenant.CreatedBy,
                ModifiedBy = tenant.ModifiedBy
            };

            return Ok(response);
        }

        [HttpPut("current/organization")]
        public async Task<IActionResult> UpdateOrganizationInfo([FromBody] UpdateOrganizationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tenant = await _tenantRepository.GetByIdAsync(_tenantContext.TenantId);
            if (tenant == null)
            {
                return NotFound();
            }

            // Update organization fields
            tenant.OrganizationName = request.OrganizationName?.Trim();
            tenant.OrganizationTagline = request.OrganizationTagline?.Trim();

            // Convert base64 logo to byte array if provided
            if (!string.IsNullOrEmpty(request.OrganizationLogoBase64))
            {
                try
                {
                    // Remove data URL prefix if present (e.g., "data:image/png;base64,")
                    var base64Data = request.OrganizationLogoBase64;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }
                    tenant.OrganizationLogo = Convert.FromBase64String(base64Data);
                }
                catch (FormatException)
                {
                    return BadRequest("Invalid logo image format");
                }
            }
            else if (request.OrganizationLogoBase64 == null)
            {
                // Explicitly set to null if no logo provided
                tenant.OrganizationLogo = null;
            }

            tenant.ModifiedBy = "system"; // TODO: Get from auth context
            await _tenantRepository.UpdateAsync(tenant);
            await _unitOfWork.CommitAsync();

            return NoContent();
        }
    }
}

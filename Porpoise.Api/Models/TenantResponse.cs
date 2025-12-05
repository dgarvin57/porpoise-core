namespace Porpoise.Api.Models;

public class TenantResponse
{
    public string TenantId { get; set; } = string.Empty;  // GUID
    public string TenantKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? OrganizationName { get; set; }
    public string? OrganizationLogoBase64 { get; set; }  // Base64-encoded organization logo
    public string? OrganizationTagline { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
}

public class UpdateOrganizationRequest
{
    public string? OrganizationName { get; set; }
    public string? OrganizationLogoBase64 { get; set; }  // Base64-encoded logo
    public string? OrganizationTagline { get; set; }
}

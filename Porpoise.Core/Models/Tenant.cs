using System.Xml.Serialization;

namespace Porpoise.Core.Models;

public class Tenant
{
    public string TenantId { get; set; } = string.Empty;  // GUID stored as CHAR(36)
    public string TenantKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Organization branding (for researcher/consultant)
    public string? OrganizationName { get; set; }
    
    [XmlIgnore]
    public byte[]? OrganizationLogo { get; set; }
    
    public string? OrganizationTagline { get; set; }
    
    // Audit fields
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
}

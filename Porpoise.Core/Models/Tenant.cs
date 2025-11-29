namespace Porpoise.Core.Models;

public class Tenant
{
    public int TenantId { get; set; }
    public string TenantKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

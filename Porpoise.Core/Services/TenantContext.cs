namespace Porpoise.Core.Services;

public class TenantContext
{
    public string TenantId { get; set; } = string.Empty;  // GUID stored as string
    public string TenantKey { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

namespace Porpoise.Core.Services;

public class TenantContext
{
    public int TenantId { get; set; }
    public string TenantKey { get; set; } = string.Empty;
    public string TenantName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

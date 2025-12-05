namespace Porpoise.Api.Models;

public class ProjectResponse
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;  // GUID
    public string? ProjectName { get; set; }
    public string? ClientName { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DefaultWeightingScheme { get; set; }
    public string? ClientLogoBase64 { get; set; }  // Base64-encoded client logo for display
    public bool IsDeleted { get; set; }
    public DateTime? DeletedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

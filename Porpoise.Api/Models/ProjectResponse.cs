namespace Porpoise.Api.Models;

public class ProjectResponse
{
    public Guid Id { get; set; }
    public int TenantId { get; set; }
    public string? ProjectName { get; set; }
    public string? ClientName { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DefaultWeightingScheme { get; set; }
    public string? BrandingSettings { get; set; }
    public string? ResearcherLabel { get; set; }
    public string? ResearcherSubLabel { get; set; }
    public string? ResearcherLogoBase64 { get; set; }  // Base64-encoded image for display
    public bool IsDeleted { get; set; }
    public DateTime? DeletedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

namespace Porpoise.Api.Models;

public class UpdateProjectRequest
{
    public string? ProjectName { get; set; }
    public string? ClientName { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ResearcherLabel { get; set; }
    public string? ResearcherSubLabel { get; set; }
    public string? ResearcherLogoBase64 { get; set; }  // Base64-encoded image data
    public string? DefaultWeightingScheme { get; set; }
    public string? BrandingSettings { get; set; }
}

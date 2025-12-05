namespace Porpoise.Api.Models;

public class UpdateProjectRequest
{
    public string? ProjectName { get; set; }
    public string? ClientName { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DefaultWeightingScheme { get; set; }
    public string? ClientLogoBase64 { get; set; }  // Base64-encoded client logo
}

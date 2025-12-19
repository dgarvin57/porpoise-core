using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Porpoise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var assembly = typeof(Program).Assembly;
        var version = assembly.GetName().Version?.ToString() ?? "unknown";
        
        // Check if migration scripts are embedded
        var dataAccessAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Porpoise.DataAccess");
        
        var embeddedResources = dataAccessAssembly?.GetManifestResourceNames()
            .Where(r => r.EndsWith(".sql"))
            .ToList() ?? new List<string>();
        
        return Ok(new
        {
            status = "healthy",
            version,
            timestamp = DateTime.UtcNow,
            migrations = new
            {
                dataAccessAssemblyFound = dataAccessAssembly != null,
                embeddedMigrationCount = embeddedResources.Count,
                embeddedMigrations = embeddedResources
            }
        });
    }
}

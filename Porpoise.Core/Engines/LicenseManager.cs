#nullable enable

using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// Manages Porpoise license validation and feature unlocking.
/// Currently a placeholder — full implementation will be added when license logic is ported.
/// </summary>
public class LicenseManager
{
    // TODO: Implement license validation, feature flags, and expiration checks
    // This will likely involve:
    // - Reading encrypted license file
    // - Validating against hardware ID or online service
    // - Exposing boolean properties (e.g. IsTwoBlockEnabled, IsAnovaEnabled, etc.)

    public LicenseManager()
    {
        // Future: Load and validate license on construction
    }

    // Example future properties (to be implemented):
    // public bool IsValid { get; private set; }
    // public bool IsTwoBlockEnabled => true;  // stub
    // public bool IsAnovaEnabled => true;
    // public bool IsExportEnabled => true;
    // public DateTime? ExpirationDate { get; private set; }
}
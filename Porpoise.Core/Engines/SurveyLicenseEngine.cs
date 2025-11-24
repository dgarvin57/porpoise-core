#nullable enable

using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// Survey-level license validation engine.
/// Currently returns true (placeholder) — full implementation pending.
/// </summary>
public class SurveyLicenseEngine
{
    public SurveyLicenseEngine()
    {
        // Future: Initialize license check, read encrypted license data, etc.
    }

    /// <summary>
    /// Verifies the survey is properly licensed.
    /// TODO: Implement full license verification logic.
    /// </summary>
    public bool Verify()
    {
        // Placeholder — full implementation to be added
        return true;
    }
}
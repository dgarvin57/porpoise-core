#nullable enable

namespace Porpoise.Core.Engines;

/// <summary>
/// Represents a single respondent's paired DV/IV response data point within the VarCovar engine.
/// 
/// This lightweight data container holds:
/// • Raw X (DV) and Y (IV) response values
/// • Simulation and static weights for advanced weighted analysis
/// • All intermediate calculations required for real-time univariate and bivariate statistics
///   (mean deviations, squared deviations, covariation terms, etc.)
/// 
/// VarCovarData powers Porpoise's legendary ability to deliver publication-grade
/// correlation, significance, and confidence intervals — instantly, with full weighting support.
/// 
/// This class is the atomic unit behind one of the most sophisticated behavioral
/// analytics engines ever built into a survey platform.
/// </summary>
public class VarCovarData
{
    /// <summary>
    /// Raw DV (X) response value from the respondent
    /// </summary>
    public int XResp { get; set; }

    /// <summary>
    /// Raw IV (Y) response value from the respondent (0 if univariate)
    /// </summary>
    public int YResp { get; set; }

    /// <summary>
    /// Simulation weight for this respondent (from WeightOn question)
    /// </summary>
    public double SimWeight { get; set; } = 1.0;

    /// <summary>
    /// Static weight for this respondent (from WEIGHT column)
    /// </summary>
    public double StaticWeight { get; set; } = 1.0;

    // Weighted response values
    public double XRespTimesWeight { get; set; }
    public double YRespTimesWeight { get; set; }

    // Deviation calculations
    public double XMinusMean { get; set; }
    public double YMinusMean { get; set; }

    // Squared deviations
    public double XMinusMeanSq { get; set; }
    public double YMinusMeanSq { get; set; }

    // Weighted squared deviations
    public double XMinusMeanSqTimesWeight { get; set; }
    public double YMinusMeanSqTimesWeight { get; set; }

    // Cross-product terms for correlation
    public double XMinMeanXYMinMean { get; set; }
    public double XMinMeanXYMinMeanTimesWeight { get; set; }
}
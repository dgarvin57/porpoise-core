#nullable enable

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a single respondent's combination of DV and IV response values
/// used internally by Crosstab to build matrices and apply weights.
/// </summary>
public class CrosstabItem
{
    private int _rCase;
    private int _dvRespNumber;      // RespValue (number) of DV response so we can order correctly
    private int _ivRespNumber;      // RespValue (number) of IV response so we can order correctly
    private double _responseWeight;
    private double _staticWeight;

    public int RCase
    {
        get => _rCase;
        set => _rCase = value;
    }

    public int DVRespNumber
    {
        get => _dvRespNumber;
        set => _dvRespNumber = value;
    }

    public double ResponseWeight
    {
        get => _responseWeight;
        set => _responseWeight = value;
    }

    public double StaticWeight
    {
        get => _staticWeight;
        set => _staticWeight = value;
    }

    public int IVRespNumber
    {
        get => _ivRespNumber;
        set => _ivRespNumber = value;
    }
}
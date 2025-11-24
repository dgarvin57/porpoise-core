#nullable enable

using Porpoise.Core.Engines;
using Porpoise.Core.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Porpoise.Core.Models;

/// <summary>
/// Represents a single response option for a survey question.
/// Contains value, label, index type (+/-/neutral), frequency, percentages, and weighting.
/// </summary>
[Serializable]
public class Response : ObjectBase
{
    #region Members

    private Guid _id = Guid.NewGuid();
    private int _respValue;
    private string _label = string.Empty;
    private ResponseIndexType _indexType = ResponseIndexType.None;
    private double _resultFrequency;
    private decimal _resultPercent;
    private decimal _cumPercent;
    private decimal _inverseCumPercent;
    private double _samplingError;
    private double _weight = 1.0;

    #endregion

    #region Constructors

    public Response() { }

    public Response(int respValue, string label, ResponseIndexType indexType)
    {
        _respValue = respValue;
        _label = label;
        _indexType = indexType;
    }

    public Response(int respValue, string label, ResponseIndexType indexType, double weight)
        : this(respValue, label, indexType)
    {
        _weight = weight;
    }

    #endregion

    #region Properties

    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value, nameof(Id));
    }

    public int RespValue
    {
        get => _respValue;
        set => SetProperty(ref _respValue, value, nameof(RespValue));
    }

    public string Label
    {
        get => _label;
        set => SetProperty(ref _label, value, nameof(Label));
    }

    public ResponseIndexType IndexType
    {
        get => _indexType;
        set => SetProperty(ref _indexType, value, nameof(IndexType));
    }

    public string IndexTypeDesc
    {
        get => _indexType switch
        {
            ResponseIndexType.None => "",
            ResponseIndexType.Neutral => "/",
            ResponseIndexType.Positive => "+",
            _ => "-"
        };
        set => _indexType = value switch
        {
            " " => ResponseIndexType.None,
            "/" => ResponseIndexType.Neutral,
            "+" => ResponseIndexType.Positive,
            "-" => ResponseIndexType.Negative,
            _ => _indexType
        };
    }

    public double ResultFrequency
    {
        get => _resultFrequency;
        set => SetProperty(ref _resultFrequency, value, nameof(ResultFrequency));
    }

    public decimal ResultPercent
    {
        get => _resultPercent;
        set => SetProperty(ref _resultPercent, value, nameof(ResultPercent));
    }

    public double ResultPercent100 => (double)_resultPercent * 100;

    public decimal CumPercent
    {
        get => _cumPercent;
        set => SetProperty(ref _cumPercent, value, nameof(CumPercent));
    }

    public decimal InverseCumPercent
    {
        get => _inverseCumPercent;
        set => SetProperty(ref _inverseCumPercent, value, nameof(InverseCumPercent));
    }

    public double SamplingError
    {
        get => _samplingError;
        set => SetProperty(ref _samplingError, value, nameof(SamplingError));
    }

    public double Weight
    {
        get => _weight;
        set => SetProperty(ref _weight, value, nameof(Weight));
    }
    public VarCovarData? VarCovarData { get; set; }
    
    #endregion

    #region Validation Methods

    public void ValidateWeight()
    {
        if (Weight < 0)
            throw new ArgumentNullException(nameof(Weight), "Response Weight must be greater than or equal to 0");
    }

    #endregion

    #region Methods

    // Gather all colors and return in a byte array (legacy licensing obfuscation)
    public static byte[] GetColors()
    {
        var colorArray = new List<byte>();

        colorArray.AddRange(CxIVIndex.GetYellow());
        //colorArray.AddRange(VarCovarData.GetBlue());
        colorArray.AddRange(IOUtils.GetRed());
        colorArray.AddRange(SurveySummary.GetBlack());

        return colorArray.ToArray();
    }

    #endregion
}
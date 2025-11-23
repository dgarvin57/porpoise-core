#nullable enable

using Porpoise.Core.Engines;
using System;
using System.Collections.Generic;
using System.Data;

namespace Porpoise.Core.Models;

/// <summary>
/// Given an interval DV and a nominal IV and survey data, calculates statistics
/// and generates a table of means
/// </summary>
public class Anova : IDisposable
{
    #region Private Members

    private List<VarCovar>? _varCovarList;
    private VarCovar? _totalVarCovar;
    private Question? _intervalDV;
    private Question? _nominalIV;
    private readonly SurveyData _data;
    private DataTable? _meansTable;
    private double _overallMean;
    private double _anovaF;
    private string _significance = string.Empty;
    private List<List<double>>? _fDist01;
    private List<List<double>>? _fDist05;
    private double _eta;
    private double _etaSq;
    private bool _disposed = false;

    #endregion

    #region Public Properties

    public Question? IntervalDV
    {
        get => _intervalDV;
        set => _intervalDV = value;
    }

    public Question? NominalIV
    {
        get => _nominalIV;
        set => _nominalIV = value;
    }

    public DataTable? MeansTable
    {
        get => _meansTable;
        set => _meansTable = value;
    }

    public double OverallMean
    {
        get => _overallMean;
        set => _overallMean = value;
    }

    public double AnovaF
    {
        get => _anovaF;
        set => _anovaF = value;
    }

    public string Significance
    {
        get => _significance;
        set => _significance = value;
    }

    public double Eta
    {
        get => _eta;
        set => _eta = value;
    }

    public double EtaSq
    {
        get => _etaSq;
        set => _etaSq = value;
    }

    #endregion

    #region Constructors

    public Anova(Question intervalDV, Question nominalIV, SurveyData surveyData)
    {
        _intervalDV = intervalDV;
        _nominalIV = nominalIV;
        _data = surveyData;
        LoadData();
        Calculate();
        CreateMeansTable();
    }

    #endregion

    #region Private Methods

    private void LoadData()
    {
        _totalVarCovar = new VarCovar(_intervalDV!, _data);
        _varCovarList = new List<VarCovar>();

        foreach (var resp in _nominalIV!.Responses)
        {
            var allDataByIVResponse = _data.Clone();
            var selectedQ = _nominalIV.Clone();

            for (int i = selectedQ.Responses.Count - 1; i >= 0; i--)
            {
                if (selectedQ.Responses[i].RespValue != resp.RespValue)
                    selectedQ.Responses.RemoveAt(i);
            }

            allDataByIVResponse.SelectedQuestion = selectedQ;
            allDataByIVResponse.SelectOn = true;

            var ivVarCovar = new VarCovar(_intervalDV!, allDataByIVResponse);
            _varCovarList.Add(ivVarCovar);
        }
    }

    private void Calculate()
    {
        int k = _varCovarList!.Count;
        double n = _data.WeightOn ? _totalVarCovar!.XWeightedN : _totalVarCovar!.XN;

        double withinGroupVariation = _varCovarList.Sum(c => c.XVariation);
        double betweenGroupVariation = _totalVarCovar!.XVariation - withinGroupVariation;

        double withinGroupVariance = withinGroupVariation / (n - k);
        double betweenGroupVariance = betweenGroupVariation / (k - 1);

        _anovaF = betweenGroupVariance / withinGroupVariance;

        _etaSq = betweenGroupVariation / _totalVarCovar.XVariation;
        _eta = Math.Sqrt(_etaSq);

        _overallMean = _data.WeightOn ? _totalVarCovar.XWeightedMean : _totalVarCovar.XMean;

        int dfN = Normalize(k - 1);
        int dfD = Normalize((int)n - k);

        BuildDistLists();

        if (_anovaF > _fDist01![dfN][dfD])
            _significance = "Significant (p<.01)";
        else if (_anovaF > _fDist05![dfN][dfD])
            _significance = "Significant (p<.05)";
        else
            _significance = "Not significant";
    }

    private static int Normalize(int x)
    {
        return x <= 5 ? x - 1 :
               x <= 7 ? 5 :
               x <= 10 ? 6 :
               x <= 15 ? 7 :
               x <= 20 ? 8 :
               x <= 30 ? 9 :
               x <= 60 ? 10 :
               x <= 120 ? 11 :
               x <= 500 ? 12 :
               x <= 1000 ? 13 : 13;
    }

    private void BuildDistLists()
    {
        // Full original F-distribution tables restored
        var df01 = new List<List<double>>
        {
            new() {4052.2, 4999.5, 5403.4, 5624.6, 5763.6, 5928.4, 6055.8, 6157.3, 6208.7, 6260.6, 6313.0, 6339.4, 6359.5, 6362.7},
            new() {98.503, 99.0, 99.166, 99.249, 99.299, 99.356, 99.399, 99.43, 99.446, 99.462, 99.479, 99.487, 99.494, 99.495},
            new() {34.116, 30.816, 29.457, 28.845, 28.429, 28.026, 27.672, 27.345, 27.049, 26.785, 26.553, 26.35, 26.172, 26.166},
            new() {21.198, 18.0, 16.694, 16.044, 15.642, 15.256, 14.915, 14.612, 14.339, 14.094, 13.88, 13.69, 13.52, 13.516},
            new() {16.258, 13.274, 12.06, 11.483, 11.142, 10.816, 10.533, 10.282, 10.057, 9.858, 9.686, 9.535, 9.403, 9.399},
            new() {13.745, 10.925, 9.78, 9.24, 8.92, 8.61, 8.34, 8.1, 7.88, 7.68, 7.51, 7.35, 7.21, 7.206},
            new() {12.246, 9.547, 8.451, 7.939, 7.636, 7.34, 7.08, 6.85, 6.64, 6.44, 6.27, 6.11, 5.97, 5.964},
            new() {11.259, 8.649, 7.591, 7.106, 6.814, 6.527, 6.27, 6.04, 5.83, 5.63, 5.46, 5.3, 5.16, 5.153},
            new() {10.565, 8.022, 6.993, 6.52, 6.239, 5.96, 5.71, 5.48, 5.27, 5.07, 4.9, 4.74, 4.6, 4.592},
            new() {10.044, 7.559, 6.552, 6.091, 5.816, 5.541, 5.29, 5.06, 4.85, 4.65, 4.48, 4.32, 4.18, 4.17},
            new() {9.646, 7.206, 6.217, 5.764, 5.494, 5.22, 4.97, 4.74, 4.53, 4.33, 4.16, 4.0, 3.86, 3.848},
            new() {9.33, 6.927, 5.953, 5.507, 5.24, 4.96, 4.71, 4.48, 4.27, 4.07, 3.9, 3.74, 3.6, 3.587},
            new() {9.074, 6.701, 5.739, 5.298, 5.033, 4.757, 4.51, 4.28, 4.07, 3.87, 3.7, 3.54, 3.4, 3.385},
            new() {8.862, 6.515, 5.564, 5.126, 4.862, 4.586, 4.34, 4.11, 3.9, 3.7, 3.53, 3.37, 3.23, 3.212}
        };
        _fDist01 = df01;

        var df05 = new List<List<double>>
        {
            new() {161.45, 199.5, 215.71, 224.58, 230.16, 236.77, 241.88, 245.95, 248.01, 250.1, 252.2, 253.25, 254.06, 254.19},
            new() {18.513, 19.0, 19.164, 19.247, 19.296, 19.353, 19.396, 19.429, 19.446, 19.462, 19.479, 19.487, 19.494, 19.495},
            new() {10.128, 9.5522, 9.2766, 9.1172, 9.0135, 8.8867, 8.7855, 8.7028, 8.6602, 8.6165, 8.572, 8.5493, 8.532, 8.5292},
            new() {7.7086, 6.9443, 6.5915, 6.3882, 6.256, 6.0942, 5.9644, 5.8579, 5.8026, 5.7458, 5.6877, 5.658, 5.6352, 5.6317},
            new() {6.6078, 5.7862, 5.4095, 5.1922, 5.0504, 4.8759, 4.7351, 4.6187, 4.5582, 4.4958, 4.4314, 4.3985, 4.3731, 4.3691},
            new() {5.9874, 5.1433, 4.7571, 4.5337, 4.3874, 4.2403, 4.1028, 3.9823, 3.898, 3.8123, 3.7259, 3.6905, 3.6638, 3.6597},
            new() {5.5914, 4.7375, 4.3469, 4.1202, 3.9715, 3.7871, 3.6366, 3.5108, 3.4445, 3.3758, 3.3043, 3.2675, 3.2388, 3.2344},
            new() {5.3176, 4.459, 4.0662, 3.8378, 3.6867, 3.4995, 3.3465, 3.2186, 3.1504, 3.0792, 3.0055, 2.9673, 2.9378, 2.933},
            new() {5.1173, 4.2565, 3.8627, 3.6328, 3.4796, 3.289, 3.1336, 3.0036, 2.9338, 2.8604, 2.7843, 2.7447, 2.7135, 2.708},
            new() {4.9645, 4.1028, 3.7082, 3.478, 3.3259, 3.1354, 2.9782, 2.845, 2.7741, 2.6996, 2.621, 2.5801, 2.5482, 2.543},
            new() {4.8443, 3.9816, 3.5865, 3.3555, 3.2025, 3.0105, 2.8515, 2.7162, 2.6435, 2.5671, 2.4868, 2.4442, 2.4107, 2.4047},
            new() {4.7472, 3.8848, 3.4888, 3.256, 3.1017, 2.9078, 2.7465, 2.6093, 2.5347, 2.4559, 2.3735, 2.3287, 2.2928, 2.286},
            new() {4.6672, 3.8046, 3.4078, 3.1734, 3.0177, 2.8218, 2.659, 2.5197, 2.4433, 2.3625, 2.2776, 2.2306, 2.1925, 2.185},
            new() {4.6001, 3.7376, 3.3396, 3.1039, 2.9466, 2.7485, 2.5837, 2.4419, 2.3635, 2.2805, 2.193, 2.1438, 2.1033, 2.095}
        };
        _fDist05 = df05;
    }

    private void CreateMeansTable()
    {
        var table = new DataTable();
        table.Columns.Add("IV");
        table.Columns.Add("Mean");
        table.Columns.Add("N");
        table.Columns.Add("SDev");
        table.Columns.Add("SErr");
        table.Columns.Add("CI");

        for (int i = 0; i < _varCovarList!.Count; i++)
        {
            var row = table.NewRow();
            row["IV"] = _nominalIV!.Responses[i].Label;
            row["Mean"] = string.Format("{0:#.#0}",
                _varCovarList[i].SimWeighted || _varCovarList[i].UseStaticWeight
                    ? _varCovarList[i].XWeightedMean
                    : _varCovarList[i].XMean);
            row["N"] = string.Format("{0:#0}",
                _varCovarList[i].SimWeighted || _varCovarList[i].UseStaticWeight
                    ? _varCovarList[i].XWeightedN
                    : _varCovarList[i].XN);
            row["SDev"] = string.Format("{0:0.#0}", _varCovarList[i].XStDev);
            row["SErr"] = string.Format("{0:0.#0}", _varCovarList[i].XStError);
            row["CI"] = string.Format("+/- {0:0.#0}", _varCovarList[i].XCI);
            table.Rows.Add(row);
        }

        _meansTable = table;
    }

    #endregion

    #region IDisposable Implementation

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // managed objects
            }

            if (_varCovarList != null)
            {
                foreach (var v in _varCovarList)
                    v?.Dispose();
                _varCovarList.Clear();
                _varCovarList = null;
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Public Methods

    public static byte[] GetGreen() => new byte[] { 0xE1, 0xD6, 0x41, 0x77 };

    #endregion
}
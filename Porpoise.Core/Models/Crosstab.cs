#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Porpoise.Core.Models;

/// <summary>
/// Performs crosstab analysis between a dependent variable (DV) and independent variable (IV).
/// Calculates counts, percentages, indices, marginal percentages, chi-square, phi, Cramér's V, etc.
/// </summary>
public class Crosstab
{
    #region Private Members

    private List<CrosstabItem> _pivotList = [];
    private readonly bool _simWeightIsOn;
    private readonly bool _useStaticWeight;
    private readonly SurveyData _surveyData;
    private readonly Question _depVarQ;
    private readonly Question _indVarQ;
    private List<List<int>>? _cxMatrix;
    private DataTable? _cxTable;
    private readonly List<CxIVIndex> _cxIVIndexes = [];
    private double _chiSquare;
    private double _pValue;
    private string _significant = string.Empty;
    private double _phi;
    private double _contingencyCoefficient;
    private double _cramersV;
    private int _totalN;
    private readonly bool _showCount;
    private readonly bool _onProfileTab;
    private List<List<int>>? _cxMatrixSave;

    #endregion

    #region Public Members

    public List<CrosstabItem> PivotList
    {
        get => _pivotList;
        set => _pivotList = value;
    }

    public double ChiSquare
    {
        get => _chiSquare;
        set => _chiSquare = value;
    }

    public double PValue
    {
        get => _pValue;
        set => _pValue = value;
    }

    public string Significant
    {
        get => _significant;
        set => _significant = value;
    }

    public double Phi
    {
        get => _phi;
        set => _phi = value;
    }

    public double ContingencyCoefficient
    {
        get => _contingencyCoefficient;
        set => _contingencyCoefficient = value;
    }

    public double CramersV
    {
        get => _cramersV;
        set => _cramersV = value;
    }

    public int TotalN
    {
        get => _totalN;
        set => _totalN = value;
    }

    public DataTable? CxTable
    {
        get
        {
            CreateCxTable();
            return _cxTable;
        }
    }

    public List<CxIVIndex> CxIVIndexes
    {
        get => _cxIVIndexes;
        set => _cxIVIndexes.ClearAndAddRange(value);
    }

    #endregion

    #region Constructor

    public Crosstab(SurveyData surveyData, Question depVarQuestion, Question indVarQuestion, bool showCount, bool onProfileTab)
    {
        _surveyData = surveyData;
        _simWeightIsOn = surveyData.WeightOn;
        _useStaticWeight = surveyData.UseStaticWeight;
        _depVarQ = depVarQuestion;
        _indVarQ = indVarQuestion;
        _showCount = showCount;
        _onProfileTab = onProfileTab;

        ArgumentNullException.ThrowIfNull(_depVarQ, "Dependent variable question is required");
        ArgumentNullException.ThrowIfNull(_indVarQ, "Independent variable question is required");
        ArgumentNullException.ThrowIfNull(_surveyData.DataList, "Survey data list is required");

        CreateCxMatrix();
        CalculateStats();
    }

    #endregion

    #region Methods

    private void CreateCxTable()
    {
        var table = new DataTable();
        table.Columns.Add("");
        foreach (var response in _indVarQ.Responses)
            table.Columns.Add(response.Label);

        for (int cxMatrixRow = 0; cxMatrixRow < _cxMatrix!.Count; cxMatrixRow++)
        {
            var row = table.NewRow();
            row[0] = _depVarQ.Responses[cxMatrixRow].Label;

            for (int cxMatrixCol = 0; cxMatrixCol < _cxMatrix[0].Count; cxMatrixCol++)
            {
                int sum = AddCxMatrixColumn(cxMatrixCol);
                int value = _cxMatrix[cxMatrixRow][cxMatrixCol];
                double percent = sum > 0 ? (value / (double)sum) * 100 : 0;

                row[cxMatrixCol + 1] = _showCount
                    ? value
                    : string.Format("{0:#0.0}%", percent);
            }
            table.Rows.Add(row);
        }

        // Index row
        var indexRow = table.NewRow();
        indexRow[0] = " Index";
        CalculateCxIndex();
        if (_cxIVIndexes.Count == 0)
            throw new ArgumentOutOfRangeException("Crosstab.CalculateCxIndex() failed to calculate index");

        for (int i = 0; i < _cxIVIndexes.Count; i++)
            indexRow[i + 1] = _cxIVIndexes[i].Index;
        table.Rows.Add(indexRow);

        // Marginal percentage row
        var mRow = table.NewRow();
        mRow[0] = " Marginal Percentage";
        var listMPs = CalculateCxMarginalPercentage();
        for (int i = 0; i < listMPs.Count; i++)
            mRow[i + 1] = listMPs[i];
        table.Rows.Add(mRow);

        _cxTable = table;
    }

    private int AddCxMatrixColumn(int col)
    {
        int sum = 0;
        var matrix = _onProfileTab && _surveyData.SelectPlusOn ? _cxMatrixSave! : _cxMatrix!;
        for (int i = 0; i < matrix.Count; i++)
            sum += matrix[i][col];
        return sum;
    }

    private int AddCxMatrixRow(int row)
    {
        int sum = 0;
        for (int i = 0; i < _cxMatrix![0].Count; i++)
            sum += _cxMatrix[row][i];
        return sum;
    }

    private void CalculateCxIndex()
    {
        _cxIVIndexes.Clear();

        for (int cxCol = 0; cxCol < _cxMatrix![0].Count; cxCol++)
        {
            int sum = AddCxMatrixColumn(cxCol);
            double positive = 0, negative = 0, neutral = 0;

            for (int cxRow = 0; cxRow < _cxMatrix.Count; cxRow++)
            {
                int value = _cxMatrix[cxRow][cxCol];
                double percent = sum > 0 ? (value / (double)sum) * 100 : 0;
                var iType = _depVarQ.Responses[cxRow].IndexType;

                if (iType == ResponseIndexType.Positive) positive += percent;
                else if (iType == ResponseIndexType.Negative) negative += percent;
                else if (iType == ResponseIndexType.Neutral) neutral += percent;
            }

            var thisIndex = new CxIVIndex
            {
                IVLabel = _indVarQ.Responses[cxCol].Label,
                PosIndex = positive,
                NegIndex = negative,
                NeutralIndex = neutral,
                Index = (int)Math.Ceiling(positive - negative + 100)
            };
            _cxIVIndexes.Add(thisIndex);
        }
    }

    private List<string> CalculateCxMarginalPercentage()
    {
        List<string> listOfMPs = [];
        int totN = 0;

        var matrix = _onProfileTab && _surveyData.SelectPlusOn ? _cxMatrixSave! : _cxMatrix!;

        for (int col = 0; col < matrix[0].Count; col++)
            totN += AddCxMatrixColumn(col);

        for (int col = 0; col < matrix[0].Count; col++)
        {
            int sum = AddCxMatrixColumn(col);
            double mp = totN > 0 ? sum / (double)totN : 0;
            listOfMPs.Add(mp > 0 ? string.Format("{0:#0.0}%", mp * 100) : "0");
        }
        return listOfMPs;
    }

    private List<double> CalculateCxMarginalPercentageDbl()
    {
        List<double> result = [];
        foreach (var s in CalculateCxMarginalPercentage())
        {
            if (double.TryParse(s.TrimEnd('%'), out double val))
                result.Add(val / 100);
        }
        return result;
    }

    private void CreateCxMatrix()
    {
        if (_onProfileTab && _surveyData.SelectPlusOn)
        {
            _surveyData.SelectPlusOn = false;
            var itemListSave = _surveyData.GetCleanDVIVDataFromSurvey(_depVarQ, _indVarQ);
            _surveyData.SelectPlusOn = true;
            ConvertPivotListToCxMatrixSave(itemListSave);
        }

        _pivotList = _surveyData.GetCleanDVIVDataFromSurvey(_depVarQ, _indVarQ);
        ConvertPivotListToCxMatrix();
    }

    private void ConvertPivotListToCxMatrix()
    {
        _cxMatrix = [];

        var distinctDV = _depVarQ.Responses.Select(r => r.RespValue).Distinct().OrderBy(x => x).ToList();
        var distinctIV = _indVarQ.Responses.Select(r => r.RespValue).Distinct().OrderBy(x => x).ToList();

        foreach (int dv in distinctDV)
        {
            List<int> row = [];
            foreach (int iv in distinctIV)
            {
                double count = _pivotList
                    .Where(item => item.DVRespNumber == dv && item.IVRespNumber == iv)
                    .Sum(item =>
                    {
                        double add = 1.0;
                        if (_simWeightIsOn && _surveyData.WeightedQuestion != null && !_surveyData.WeightedQuestion.Equals(_depVarQ))
                            add *= item.ResponseWeight;
                        if (_useStaticWeight)
                            add *= item.StaticWeight;
                        return add;
                    });
                row.Add((int)count);
            }
            _cxMatrix.Add(row);
        }
    }

    private void ConvertPivotListToCxMatrixSave(List<CrosstabItem> usePivotList)
    {
        _cxMatrixSave = [];

        var distinctDV = _depVarQ.Responses.Select(r => r.RespValue).Distinct().OrderBy(x => x).ToList();
        var distinctIV = _indVarQ.Responses.Select(r => r.RespValue).Distinct().OrderBy(x => x).ToList();

        foreach (int dv in distinctDV)
        {
            List<int> row = [];
            foreach (int iv in distinctIV)
            {
                double count = usePivotList
                    .Where(item => item.DVRespNumber == dv && item.IVRespNumber == iv)
                    .Sum(item =>
                    {
                        double add = 1.0;
                        if (_simWeightIsOn && _surveyData.WeightedQuestion != null && !_surveyData.WeightedQuestion.Equals(_depVarQ))
                            add *= item.ResponseWeight;
                        if (_useStaticWeight)
                            add *= item.StaticWeight;
                        return add;
                    });
                row.Add((int)count);
            }
            _cxMatrixSave.Add(row);
        }
    }

    public StatSigItem GetStatSigItems()
    {
        return new StatSigItem
        {
            Id = _indVarQ.Id,
            QuestionLabel = _indVarQ.QstLabel,
            Significance = _significant,
            Phi = _phi
        };
    }

    public List<IndexItem> GetIndexesList()
    {
        List<IndexItem> indexes = [];

        foreach (DataRow dr in CxTable!.Rows)
        {
            if (dr[0].ToString()?.ToUpper().Contains("INDEX", StringComparison.CurrentCultureIgnoreCase) == true)
            {
                for (int i = 1; i < dr.ItemArray.Length; i++)
                {
                    indexes.Add(new IndexItem
                    {
                        Index = Convert.ToInt32(dr[i]),
                        ResponseLabel = CxTable.Columns[i].ColumnName,
                        QuestionLabel = _indVarQ.QstLabel,
                        Id = _indVarQ.Id
                    });
                }
                break;
            }
        }
        return indexes;
    }

    public List<ProfileItem>? GetProfilePercentages(int respIndex)
    {
        if (_cxMatrix is null)
            throw new ArgumentNullException(nameof(respIndex), "CxMatrix cannot be null. Instantiate the Crosstab object");

        if (respIndex < 0 || respIndex >= _cxMatrix.Count)
            return null;

        List<ProfileItem> percentages = [];
        var mps = CalculateCxMarginalPercentageDbl();
        int rowSum = AddCxMatrixRow(respIndex);

        for (int cxCol = 0; cxCol < _cxMatrix[0].Count; cxCol++)
        {
            var profile = new ProfileItem
            {
                PercNum = _cxMatrix[respIndex][cxCol] / (double)rowSum,
                MarginalPercent = cxCol < mps.Count ? mps[cxCol] : 0,
                PercDiff = 0, // will be calculated by caller if needed
                ResponseLabel = _indVarQ.Responses[cxCol].Label,
                QuestionLabel = _indVarQ.QstLabel,
                Id = _indVarQ.Id
            };
            profile.PercDiff = profile.PercNum - profile.MarginalPercent;
            percentages.Add(profile);
        }
        return percentages;
    }

    private void CalculateStats()
    {
        int rowCount = _cxMatrix!.Count;
        int colCount = _cxMatrix[0].Count;

        for (int x = 0; x < rowCount; x++)
            for (int y = 0; y < colCount; y++)
                _totalN += _cxMatrix[x][y];

        int totalCells = rowCount * colCount;
        var chiMatrix = new double[totalCells, 5];
        int chiRow = 0;

        for (int row = 0; row < rowCount; row++)
        {
            int rowSum = _cxMatrix[row].Sum();
            for (int col = 0; col < colCount; col++)
            {
                chiMatrix[chiRow, 0] = row + 1;
                chiMatrix[chiRow, 1] = col + 1;
                chiMatrix[chiRow, 2] = _cxMatrix[row][col];

                int colSum = 0;
                for (int r = 0; r < rowCount; r++) colSum += _cxMatrix[r][col];

                chiMatrix[chiRow, 3] = (rowSum * colSum) / (double)_totalN;
                chiMatrix[chiRow, 4] = Math.Pow(chiMatrix[chiRow, 2] - chiMatrix[chiRow, 3], 2) / chiMatrix[chiRow, 3];
                chiRow++;
            }
        }

        _chiSquare = chiMatrix.Cast<double>().Where((_, i) => i % 5 == 4).Sum();

        int chiDf = (rowCount - 1) * (colCount - 1);
        if (chiDf > 25) chiDf = 25;

        float[,] chiDistribution = {
            {0, 0}, {3.84f, 6.64f}, {5.99f, 9.21f}, {7.82f, 11.35f}, {9.49f, 13.28f}, {11.07f, 15.09f},
            {12.59f, 16.81f}, {14.07f, 18.48f}, {15.51f, 20.09f}, {16.92f, 21.67f}, {18.31f, 23.21f},
            {19.68f, 24.73f}, {21.03f, 26.22f}, {22.36f, 27.69f}, {23.69f, 29.14f}, {25.0f, 30.58f},
            {26.3f, 32.0f}, {27.59f, 33.41f}, {28.87f, 34.81f}, {30.14f, 36.19f}, {31.41f, 37.57f},
            {32.67f, 38.93f}, {33.92f, 40.29f}, {35.17f, 41.64f}, {36.42f, 42.9f}, {37.65f, 44.31f}
        };

        // Calculate approximate p-value using chi-square distribution
        // This is a simplified approximation based on critical values
        if (_chiSquare > chiDistribution[chiDf, 1])
        {
            _significant = "Significant (p<.01)";
            _pValue = 0.005; // Approximate: less than .01
        }
        else if (_chiSquare > chiDistribution[chiDf, 0])
        {
            _significant = "Significant (p<.05)";
            _pValue = 0.03; // Approximate: between .01 and .05
        }
        else
        {
            _significant = "Not significant";
            _pValue = 0.10; // Approximate: greater than .05
        }

        _phi = Math.Sqrt(_chiSquare / _totalN);
        _contingencyCoefficient = Math.Sqrt(_chiSquare / (_totalN + _chiSquare));

        int k = Math.Min(rowCount, colCount);
        _cramersV = Math.Sqrt(_chiSquare / (_totalN * (k - 1)));
    }

    public static string GetSupport2() => "1Y4r0UMguKDIOlNZjki9XyB7cI=";

    #endregion
}

// Small helper extension to make Clear+AddRange cleaner
internal static class ListExtensions
{
    public static void ClearAndAddRange<T>(this List<T> list, IEnumerable<T> items)
    {
        list.Clear();
        list.AddRange(items);
    }
}
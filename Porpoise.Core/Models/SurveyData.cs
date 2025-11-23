#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Porpoise.Core.Models;

/// <summary>
/// Core class representing the raw survey response data (as List<List<string>>).
/// Handles filtering (Select On / Select Plus), weighting, movement, and conversion helpers.
/// One of the most complex and central classes in the entire engine.
/// </summary>
[Serializable]
public class SurveyData : ObjectBase, ICloneable
{
    #region Private Members

    private List<List<string>> _dataList = new();
    private readonly List<int> _missingResponseValues = new();
    private string _dataFilePath = string.Empty;
    private bool _selectOn;
    private Question? _selectedQuestion;
    private List<List<string>>? _selectOnDataList;
    private bool _selectPlusOn;
    private SelectPlusConditionType _selectPlusCondition;
    private Question? _selectPlusQ1;
    private Question? _selectPlusQ2;
    private List<List<string>>? _selectPlusOnDataList;
    private bool _weightOn;
    private Question? _weightedQuestion;
    private bool _hasStaticWeightColumn;
    private bool _useStaticWeight;

    #endregion

    #region Constructors

    public SurveyData() { }

    public SurveyData(List<List<string>> list)
    {
        DataList = list;
        AddWeightsToDataList(DataList);
    }

    #endregion

    #region Public Properties

    [XmlIgnore]
    public List<List<string>> DataList
    {
        get
        {
            if ((_selectOn && _selectOnDataList is not null) || (_selectPlusOn && _selectOnDataList is not null))
            {
                return _selectOnDataList?.Any() == true ? _selectOnDataList : _dataList;
            }
            return _dataList;
        }
        set
        {
            _dataList = value;
            SetProperty(_dataList, value, "DataOnly");
        }
    }

    [XmlIgnore]
    public List<List<string>>? SelectOnDataList
    {
        get => _selectOnDataList;
        set => SetProperty(ref _selectOnDataList, value, nameof(SelectOnDataList));
    }

    public List<int> MissingResponseValues
    {
        get => _missingResponseValues;
        set => SetProperty(_missingResponseValues, value, nameof(MissingResponseValues));
    }

    public string DataFilePath
    {
        get => _dataFilePath;
        set => SetProperty(ref _dataFilePath, value, nameof(DataFilePath));
    }

    [XmlIgnore]
    public bool SelectOn
    {
        get => _selectOn;
        set
        {
            _selectOn = false;
            if (value)
            {
                if (_selectedQuestion?.Responses?.Count > 0)
                    _selectOn = true;
            }
            _selectOnDataList = GetFilteredDataList();
        }
    }

    [XmlIgnore]
    public bool SelectPlusOn
    {
        get => _selectPlusOn;
        set
        {
            _selectPlusOn = false;
            if (value)
            {
                if (_selectPlusQ1?.Responses?.Count > 0 && _selectPlusQ2?.Responses?.Count > 0)
                    _selectPlusOn = value;
            }
            AddMovementToDataList();
            SelectOnDataList = GetFilteredDataList();
        }
    }

    [XmlIgnore]
    public int OriginalTotalN => _dataList.Count;

    [XmlIgnore]
    public Question? SelectedQuestion
    {
        get => _selectedQuestion;
        set => SetProperty(ref _selectedQuestion, value, nameof(SelectedQuestion));
    }

    [XmlIgnore]
    public Question? SelectPlusQ1
    {
        get => _selectPlusQ1;
        set => SetProperty(ref _selectPlusQ1, value, nameof(SelectPlusQ1));
    }

    [XmlIgnore]
    public Question? SelectPlusQ2
    {
        get => _selectPlusQ2;
        set => SetProperty(ref _selectPlusQ2, value, nameof(SelectPlusQ2));
    }

    [XmlIgnore]
    public SelectPlusConditionType SelectPlusCondition
    {
        get => _selectPlusCondition;
        set => SetProperty(ref _selectPlusCondition, value, nameof(SelectPlusCondition));
    }

    [XmlIgnore]
    public bool WeightOn
    {
        get => _weightOn;
        set
        {
            _weightOn = false;
            if (value && _weightedQuestion?.Responses?.Count > 0)
            {
                _weightOn = true;
                var list = _selectOn ? _selectOnDataList : _dataList;
                if (list is not null) AddWeightsToDataList(list);
            }
        }
    }

    public bool UseStaticWeight
    {
        get => _useStaticWeight;
        set
        {
            _useStaticWeight = false;
            if (value)
            {
                _useStaticWeight = true;
                var list = _selectOn ? _selectOnDataList : _dataList;
                if (list is not null) AddWeightsToDataList(list);
            }
        }
    }

    public bool HasStaticWeightColumn => GetStaticWeightColumnNumber() > 0;

    [XmlIgnore]
    public Question? WeightedQuestion
    {
        get => _weightedQuestion;
        set => _weightedQuestion = value;
    }

    #endregion

    #region ICloneable

    public object Clone()
    {
        var newClone = new SurveyData();

        newClone.MissingResponseValues = new List<int>(_missingResponseValues);
        newClone.DataFilePath = _dataFilePath;

        if (_selectOn && _selectedQuestion is not null)
            newClone.SelectedQuestion = (Question?)_selectedQuestion.Clone();

        if (_weightedQuestion is not null)
            newClone.WeightedQuestion = (Question?)_weightedQuestion.Clone();

        var dlClone = new List<List<string>>();
        foreach (var row in _dataList)
            dlClone.Add(new List<string>(row));

        newClone.DataList = dlClone;
        newClone.SelectOn = _selectOn;
        newClone.WeightOn = _weightOn;
        newClone.UseStaticWeight = _useStaticWeight;

        return newClone;
    }

    #endregion

    #region Standard Methods

    public DataTable ToDataTable(bool includeQstNumber, bool includeWeights)
    {
        var table = new DataTable();
        bool listContainsWeightsCol = false;
        int startRow = includeQstNumber ? 0 : 1;

        // Columns
        for (int i = 0; i < DataList[0].Count; i++)
            table.Columns.Add(DataList[0][i], typeof(string));

        // Rows
        for (int rowCount = startRow; rowCount < DataList.Count; rowCount++)
        {
            if (!IncludeRow(rowCount)) continue;

            var row = table.NewRow();
            for (int colCount = 0; colCount < DataList[rowCount].Count; colCount++)
            {
                string value = DataList[rowCount][colCount];
                if (value.ToUpper() == "#?SIM_WEIGHT/?#")
                {
                    listContainsWeightsCol = true;
                    if (includeWeights) row[colCount] = value;
                }
                else if (listContainsWeightsCol && colCount == DataList[rowCount].Count - 1)
                {
                    row[colCount] = value;
                }
                else if (IsResponseNumeric(rowCount, colCount))
                {
                    row[colCount] = value;
                }
            }
            table.Rows.Add(row);
        }
        return table;
    }

    public List<List<string>> DataTableToListOfList(DataTable dt)
    {
        var list = new List<List<string>>();
        var headers = new List<string>();
        foreach (DataColumn c in dt.Columns) headers.Add(c.ColumnName);
        list.Add(headers);

        foreach (DataRow r in dt.Rows)
        {
            var row = new List<string>();
            foreach (var item in r.ItemArray)
                row.Add(item?.ToString() ?? "");
            list.Add(row);
        }
        return list;
    }

    #endregion

    #region Select On / Select Plus / Weighting / Movement — ALL PRESERVED 100%

    private List<List<string>> GetFilteredDataList()
    {
        var filtered = new List<List<string>>();
        for (int i = 0; i < _dataList.Count; i++)
        {
            if (IncludeRow(i))
                filtered.Add(new List<string>(_dataList[i]));
        }
        return filtered;
    }

    private bool IncludeRow(int rowCount)
    {
        if (rowCount == 0) return true;

        bool include = true;

        if (_selectOn && !_selectPlusOn)
        {
            include = _selectedQuestion!.Responses.Any(r => r.RespValue.ToString() == _dataList[rowCount][_selectedQuestion.DataFileCol]);
        }
        else if (!_selectOn && _selectPlusOn)
        {
            include = GetMovementForRow(rowCount) == _selectPlusCondition;
        }
        else if (_selectOn && _selectPlusOn)
        {
            bool select = _selectedQuestion!.Responses.Any(r => r.RespValue.ToString() == _dataList[rowCount][_selectedQuestion.DataFileCol]);
            bool plus = GetMovementForRow(rowCount) == _selectPlusCondition;
            include = select && plus;
        }

        return include;
    }

    public void AddMovementToDataList()
    {
        if (_selectPlusQ1 == null || _selectPlusQ2 == null) return;
        AddMovementColumn();

        for (int row = 1; row < _dataList.Count; row++)
        {
            string r1 = _dataList[row][_selectPlusQ1.DataFileCol];
            string r2 = _dataList[row][_selectPlusQ2.DataFileCol];

            if (MissingResponseValues.Any(m => m.ToString() == r1 || m.ToString() == r2))
            {
                _dataList[row][_dataList[0].Count - 1] = "0";
                continue;
            }

            var idx1 = GetIndexValue(_selectPlusQ1, int.Parse(r1));
            var idx2 = GetIndexValue(_selectPlusQ2, int.Parse(r2));
            var movement = CalculateMovement(idx1, idx2);
            _dataList[row][_dataList[0].Count - 1] = ((int)movement).ToString();
        }
    }

    private void AddMovementColumn()
    {
        if (GetMovementColumnNumber() == 0)
        {
            _dataList[0].Add("#?/MOVEMENT/?#");
            for (int i = 1; i < _dataList.Count; i++)
                _dataList[i].Add("0");
        }
    }

    private int GetMovementColumnNumber()
    {
        for (int i = 0; i < _dataList[0].Count; i++)
            if (_dataList[0][i] == "#?/MOVEMENT/?#") return i;
        return 0;
    }

    private ResponseIndexType GetIndexValue(Question q, int resp)
    {
        return q.Responses.FirstOrDefault(r => r.RespValue == resp)?.IndexType ?? ResponseIndexType.None;
    }

    private SelectPlusConditionType CalculateMovement(ResponseIndexType i1, ResponseIndexType i2)
    {
        if (i1 == ResponseIndexType.None || i2 == ResponseIndexType.None) return SelectPlusConditionType.None;

        return (i1, i2) switch
        {
            (ResponseIndexType.Positive, ResponseIndexType.Positive) => SelectPlusConditionType.StaysPositive,
            (ResponseIndexType.Positive, ResponseIndexType.Negative) => SelectPlusConditionType.GoesNegative,
            (ResponseIndexType.Positive, ResponseIndexType.Neutral) => SelectPlusConditionType.GoesNegative,
            (ResponseIndexType.Negative, ResponseIndexType.Positive) => SelectPlusConditionType.GoesPositive,
            (ResponseIndexType.Negative, ResponseIndexType.Negative) => SelectPlusConditionType.StaysNegative,
            (ResponseIndexType.Negative, ResponseIndexType.Neutral) => SelectPlusConditionType.GoesPositive,
            (ResponseIndexType.Neutral, ResponseIndexType.Positive) => SelectPlusConditionType.GoesPositive,
            (ResponseIndexType.Neutral, ResponseIndexType.Negative) => SelectPlusConditionType.GoesNegative,
            (ResponseIndexType.Neutral, ResponseIndexType.Neutral) => SelectPlusConditionType.StaysNeutral,
            _ => SelectPlusConditionType.None
        };
    }

    private SelectPlusConditionType GetMovementForRow(int rowNumber)
    {
        int col = GetMovementColumnNumber();
        if (col == 0) return SelectPlusConditionType.None;
        return (SelectPlusConditionType)int.Parse(_dataList[rowNumber][col]);
    }

    private void AddWeightsToDataList(List<List<string>> list)
    {
        if (GetSimWeightColumnNumber() == 0)
        {
            list[0].Add("#?SIM_WEIGHT/?#");
            for (int i = 1; i < list.Count; i++) list[i].Add("1");
        }

        if (!_weightOn && !_useStaticWeight) return;

        for (int row = 1; row < list.Count; row++)
        {
            string resp = list[row][_weightedQuestion!.DataFileCol];
            double weight = 1.0;
            if (_weightOn)
            {
                var w = _weightedQuestion.Responses.FirstOrDefault(r => r.RespValue.ToString() == resp)?.weight ?? 1.0;
                weight *= w;
            }
            if (_useStaticWeight)
            {
                weight *= GetStaticWeight(row, true);
            }
            list[row][list[0].Count - 1] = weight.ToString();
        }
    }

    public int GetSimWeightColumnNumber()
    {
        for (int i = 0; i < _dataList[0].Count; i++)
            if (_dataList[0][i] == "#?SIM_WEIGHT/?#") return i;
        return 0;
    }

    public int GetStaticWeightColumnNumber()
    {
        for (int i = 0; i < _dataList[0].Count; i++)
            if (_dataList[0][i].ToUpper() == "WEIGHT") return i;
        return 0;
    }

    private double GetResponseSimWeight(int rowNumber) => double.TryParse(DataList[rowNumber][GetSimWeightColumnNumber()], out var w) ? w : 1.0;

    private double GetStaticWeight(int rowNumber, bool ignoreUseStaticWeight)
    {
        if (!_useStaticWeight && !ignoreUseStaticWeight) return 1.0;
        return double.TryParse(DataList[rowNumber][GetStaticWeightColumnNumber()], out var w) ? w : 1.0;
    }

    public List<int> GetAllResponsesInColumn(int colNumber, bool omitMissingValues, List<int> qMissingValues)
    {
        var list = new List<int>();
        for (int row = 1; row < DataList.Count; row++)
        {
            if (!IncludeRow(row)) continue;
            if (!int.TryParse(DataList[row][colNumber], out int val)) continue;
            if (omitMissingValues && qMissingValues.Contains(val)) continue;
            list.Add(val);
        }
        return list;
    }

    public bool IsResponseNumeric(int rowNum, int colNum)
    {
        string header = DataList[0][colNum].ToUpper();
        if (header.Contains("#?SIM_WEIGHT/?#") || header.Contains("WEIGHT")) return true;

        if (!int.TryParse(DataList[rowNum][colNum], out _))
        {
            string msg = $"The data file contains unacceptable data.{Environment.NewLine}{Environment.NewLine}" +
                         $"Question '{DataList[0][colNum]}' starting with row '{rowNum}' has invalid data ('{DataList[rowNum][colNum]}'). Data must be positive integers. Edit the data file '{DataFilePath}' to correct the invalid cells.";
            throw new SurveyResponseIsNotNumericException(msg, _dataFilePath);
        }
        return true;
    }

    public List<CrosstabItem> GetCleanDVIVDataFromSurvey(Question dvQ, Question? ivQ = null)
    {
        var list = new List<CrosstabItem>();
        bool both = ivQ is not null;

        for (int row = 1; row < DataList.Count; row++)
        {
            int dvVal = int.Parse(DataList[row][dvQ.DataFileCol]);
            int ivVal = both ? int.Parse(DataList[row][ivQ!.DataFileCol]) : 0;

            if (!GetCleanDVIVDataFromSurvey_Helper(dvVal, ivVal, dvQ, ivQ, both)) continue;

            var item = new CrosstabItem
            {
                RCase = int.Parse(DataList[row][0]),
                DVRespNumber = dvVal,
                IVRespNumber = both ? ivVal : 0,
                ResponseWeight = GetResponseSimWeight(row),
                StaticWeight = GetStaticWeight(row, true)
            };
            list.Add(item);
        }
        return list;
    }

    private static bool GetCleanDVIVDataFromSurvey_Helper(int dvValue, int ivValue, Question dvQ, Question? ivQ, bool both)
    {
        var dvResponses = dvQ.Responses.Select(r => r.RespValue).ToList();
        if (!dvResponses.Contains(dvValue)) return false;

        if (both && ivQ is not null)
        {
            var ivResponses = ivQ.Responses.Select(r => r.RespValue).ToList();
            if (!ivResponses.Contains(ivValue)) return false;
        }
        return true;
    }

    #endregion

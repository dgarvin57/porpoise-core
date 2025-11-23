#nullable enable

using System;
using System.Runtime.Serialization;

namespace Porpoise.Core.Models;

#region Custom Exception

public class SurveyDataFileIsNotCSVTypeException : Exception
{
    public SurveyDataFileIsNotCSVTypeException() { }
    public SurveyDataFileIsNotCSVTypeException(string message) : base(message) { }
    public SurveyDataFileIsNotCSVTypeException(string message, Exception? inner) : base(message, inner) { }
}

public class SurveyCaseNumberIsNotIntegerException : Exception
{
    public SurveyCaseNumberIsNotIntegerException() { }
    public SurveyCaseNumberIsNotIntegerException(string message) : base(message) { }
    public SurveyCaseNumberIsNotIntegerException(string message, Exception? inner) : base(message, inner) { }
}

[Serializable]
public class SurveyColumnAllMissingValuesException : Exception
{
    private string _surveyDataFilePath = string.Empty;
    private string _qstNum = string.Empty;
    private int _dataColNum;

    public string SurveyDataFilePath
    {
        get => _surveyDataFilePath;
        set => _surveyDataFilePath = value;
    }

    public string QstNum
    {
        get => _qstNum;
        set => _qstNum = value;
    }

    public int DataColNum
    {
        get => _dataColNum;
        set => _dataColNum = value;
    }

    public SurveyColumnAllMissingValuesException() { }

    public SurveyColumnAllMissingValuesException(string message, string surveyDataFilePath, string qstNum, int dataColNum)
        : base(message)
    {
        _surveyDataFilePath = surveyDataFilePath;
        _qstNum = qstNum;
        _dataColNum = dataColNum;
    }

    public SurveyColumnAllMissingValuesException(string message, Exception? inner, string surveydataFilePath, string qstNum, int dataColNum)
        : base(message, inner)
    {
        _surveyDataFilePath = surveydataFilePath;
        _qstNum = qstNum;
        _dataColNum = dataColNum;
    }
}

[Serializable]
public class SurveyResponseIsNotNumericException : Exception
{
    private string _surveyDataFilePath = string.Empty;

    public string SurveyDataFilePath
    {
        get => _surveyDataFilePath;
        set => _surveyDataFilePath = value;
    }

    public SurveyResponseIsNotNumericException() { }

    public SurveyResponseIsNotNumericException(string message, string surveyDataFilePath)
        : base(message)
    {
        _surveyDataFilePath = surveyDataFilePath;
    }

    public SurveyResponseIsNotNumericException(string message, Exception? inner, string surveydataFilePath)
        : base(message, inner)
    {
        _surveyDataFilePath = surveydataFilePath;
    }
}

[Serializable]
public class SurveyDataFileLoadException : Exception
{
    private string _surveyFilePath = string.Empty;
    private string _surveyDataFilePath = string.Empty;
    private Guid _surveyId;
    private string _originalDatafilePath = string.Empty;
    private bool _exportedProject;

    public string SurveyFilePath
    {
        get => _surveyFilePath;
        set => _surveyFilePath = value;
    }

    public string SurveyDataFilePath
    {
        get => _surveyDataFilePath;
        set => _surveyDataFilePath = value;
    }

    public Guid SurveyId
    {
        get => _surveyId;
        set => _surveyId = value;
    }

    public string OriginalDatafilePath
    {
        get => _originalDatafilePath;
        set => _originalDatafilePath = value;
    }

    public bool ExportedProject
    {
        get => _exportedProject;
        set => _exportedProject = value;
    }

    public SurveyDataFileLoadException() { }

    public SurveyDataFileLoadException(string message, string surveyFilePath, string surveyDataFilePath, Guid surveyId, string originalDatafilePath, bool exportedProject)
        : base(message)
    {
        _surveyFilePath = surveyFilePath;
        _surveyDataFilePath = surveyDataFilePath;
        _surveyId = surveyId;
        _originalDatafilePath = originalDatafilePath;
        _exportedProject = exportedProject;
    }

    public SurveyDataFileLoadException(string message, Exception? inner, string surveyFilePath, string surveyDataFilePath, Guid surveyId, string originalDatafilePath, bool exportedProject)
        : base(message, inner)
    {
        _surveyFilePath = surveyFilePath;
        _surveyDataFilePath = surveyDataFilePath;
        _surveyId = surveyId;
        _originalDatafilePath = originalDatafilePath;
        _exportedProject = exportedProject;
    }

    protected SurveyDataFileLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        if (info is not null)
        {
            _surveyDataFilePath = info.GetString("_surveyFilename") ?? string.Empty;
            _surveyId = new Guid(info.GetString("_surveyId") ?? Guid.Empty.ToString());
            _originalDatafilePath = info.GetString("_originalDatafilePath") ?? string.Empty;
            _surveyFilePath = info.GetString("_surveyFilePath") ?? string.Empty;
            _exportedProject = info.GetBoolean("_exportedProject");
        }
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        if (info is not null)
        {
            info.AddValue("_surveyFilename", _surveyDataFilePath);
            info.AddValue("_surveyId", _surveyId.ToString());
            info.AddValue("_originalDatafilePath", _originalDatafilePath);
            info.AddValue("_surveyFilePath", _surveyFilePath);
            info.AddValue("_exportedProject", _exportedProject);
        }
    }
}

public class DoubleBlockException : Exception
{
    public DoubleBlockException() { }
    public DoubleBlockException(string message) : base(message) { }
    public DoubleBlockException(string message, Exception? inner) : base(message, inner) { }
}

#endregion
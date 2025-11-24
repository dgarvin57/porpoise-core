#nullable enable

using System;
using System.Runtime.Serialization;

namespace Porpoise.Core.Models;

#region Custom Exceptions – ZERO serialization warnings (SYSLIB0051 + CS0672 gone)

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
public sealed class SurveyColumnAllMissingValuesException : Exception, ISerializable
{
    public string SurveyDataFilePath { get; set; } = string.Empty;
    public string QstNum { get; set; } = string.Empty;
    public int DataColNum { get; set; }

    public SurveyColumnAllMissingValuesException() { }

    public SurveyColumnAllMissingValuesException(string message, string surveyDataFilePath, string qstNum, int dataColNum)
        : base(message)
        => (SurveyDataFilePath, QstNum, DataColNum) = (surveyDataFilePath, qstNum, dataColNum);

    public SurveyColumnAllMissingValuesException(string message, Exception? inner, string surveyDataFilePath, string qstNum, int dataColNum)
        : base(message, inner)
        => (SurveyDataFilePath, QstNum, DataColNum) = (surveyDataFilePath, qstNum, dataColNum);

    private SurveyColumnAllMissingValuesException(SerializationInfo info, StreamingContext context)
        : base(info.GetString("Message") ?? string.Empty)
    {
        SurveyDataFilePath = info.GetString(nameof(SurveyDataFilePath)) ?? string.Empty;
        QstNum = info.GetString(nameof(QstNum)) ?? string.Empty;
        DataColNum = info.GetInt32(nameof(DataColNum));
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Message", Message);
        info.AddValue(nameof(SurveyDataFilePath), SurveyDataFilePath);
        info.AddValue(nameof(QstNum), QstNum);
        info.AddValue(nameof(DataColNum), DataColNum);
    }
}

[Serializable]
public sealed class SurveyResponseIsNotNumericException : Exception, ISerializable
{
    public string SurveyDataFilePath { get; set; } = string.Empty;

    public SurveyResponseIsNotNumericException() { }
    public SurveyResponseIsNotNumericException(string message, string surveyDataFilePath) : base(message)
        => SurveyDataFilePath = surveyDataFilePath;

    public SurveyResponseIsNotNumericException(string message, Exception? inner, string surveyDataFilePath) : base(message, inner)
        => SurveyDataFilePath = surveyDataFilePath;

    private SurveyResponseIsNotNumericException(SerializationInfo info, StreamingContext context)
        : base(info.GetString("Message") ?? string.Empty)
        => SurveyDataFilePath = info.GetString(nameof(SurveyDataFilePath)) ?? string.Empty;

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Message", Message);
        info.AddValue(nameof(SurveyDataFilePath), SurveyDataFilePath);
    }
}

[Serializable]
public sealed class SurveyDataFileLoadException : Exception, ISerializable
{
    public string SurveyFilePath { get; set; } = string.Empty;
    public string SurveyDataFilePath { get; set; } = string.Empty;
    public Guid SurveyId { get; set; }
    public string OriginalDatafilePath { get; set; } = string.Empty;
    public bool ExportedProject { get; set; }

    public SurveyDataFileLoadException() { }

    public SurveyDataFileLoadException(string message,
        string surveyFilePath, string surveyDataFilePath, Guid surveyId,
        string originalDatafilePath, bool exportedProject) : base(message)
        => (SurveyFilePath, SurveyDataFilePath, SurveyId, OriginalDatafilePath, ExportedProject)
            = (surveyFilePath, surveyDataFilePath, surveyId, originalDatafilePath, exportedProject);

    public SurveyDataFileLoadException(string message, Exception? inner,
        string surveyFilePath, string surveyDataFilePath, Guid surveyId,
        string originalDatafilePath, bool exportedProject) : base(message, inner)
        => (SurveyFilePath, SurveyDataFilePath, SurveyId, OriginalDatafilePath, ExportedProject)
            = (surveyFilePath, surveyDataFilePath, surveyId, originalDatafilePath, exportedProject);

    private SurveyDataFileLoadException(SerializationInfo info, StreamingContext context)
        : base(info.GetString("Message") ?? string.Empty)
    {
        SurveyFilePath = info.GetString(nameof(SurveyFilePath)) ?? string.Empty;
        SurveyDataFilePath = info.GetString(nameof(SurveyDataFilePath)) ?? string.Empty;
        SurveyId = Guid.TryParse(info.GetString(nameof(SurveyId)), out var g) ? g : Guid.Empty;
        OriginalDatafilePath = info.GetString(nameof(OriginalDatafilePath)) ?? string.Empty;
        ExportedProject = info.GetBoolean(nameof(ExportedProject));
    }

    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Message", Message);
        info.AddValue(nameof(SurveyFilePath), SurveyFilePath);
        info.AddValue(nameof(SurveyDataFilePath), SurveyDataFilePath);
        info.AddValue(nameof(SurveyId), SurveyId.ToString());
        info.AddValue(nameof(OriginalDatafilePath), OriginalDatafilePath);
        info.AddValue(nameof(ExportedProject), ExportedProject);
    }
}

public class DoubleBlockException : Exception
{
    public DoubleBlockException() { }
    public DoubleBlockException(string message) : base(message) { }
    public DoubleBlockException(string message, Exception? inner) : base(message, inner) { }
}

#endregion
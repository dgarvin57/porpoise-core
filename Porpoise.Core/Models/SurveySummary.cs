#nullable enable

using System;

namespace Porpoise.Core.Models;

/// <summary>
/// Lightweight summary of a Survey — used only for display in the old project tree.
/// Contains just name, filename, folder, and ID.
/// </summary>
[Serializable]
public class SurveySummary : ObjectBase
{
    #region Members

    private Guid _id = Guid.NewGuid();
    private string _surveyName = string.Empty;
    private string _surveyFileName = string.Empty;
    private string _surveyFolder = string.Empty;

    #endregion

    #region Constructor

    public SurveySummary() { }

    #endregion

    #region Public Properties

    public Guid Id
    {
        get => _id;
        set => _id = value;
    }

    public string SurveyName
    {
        get => _surveyName;
        set => _surveyName = value;
    }

    public string SurveyFileName
    {
        get => _surveyFileName;
        set => _surveyFileName = value;
    }

    public string SurveyFolder
    {
        get => _surveyFolder;
        set => _surveyFolder = value;
    }

    #endregion

    #region Public Methods

    // Color black — legacy licensing obfuscation
    public static byte[] GetBlack() => new byte[] { 0xBA, 0x3F, 0x23, 0x8E };

    #endregion
}
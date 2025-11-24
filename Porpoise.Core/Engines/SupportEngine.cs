#nullable enable

using System.Collections.Generic;
using Porpoise.Core.DataAccess;
using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// Support & unlock engine — handles license validation, string translation,
/// file reading, and manual survey unlocking/relocking (legacy support tool).
/// </summary>
public static class SupportEngine
{
    public static bool CheckSupportOk(string input)
        => LegacyDataAccess.CheckSupportOk(input);

    public static string TranslateOne(string input)
        => LegacyDataAccess.TranslateOne(input);

    public static string UnTranslate(string input)
        => LegacyDataAccess.TranslateBack(input);

    public static string GetProjectFile(string path)
        => LegacyDataAccess.ReadProjectSurveyFile(path);

    public static string GetSurveyFile(string path)
        => LegacyDataAccess.ReadProjectSurveyFile(path);

    public static List<List<string>> GetDataFile(string path)
        => LegacyDataAccess.ReadDataFile(path);

    public static Survey? AlterSurveyString(string path, out string errorMessage)
    {
        errorMessage = string.Empty;

        var survey = new Survey();
        if (!SurveyEngine.LoadSurvey(survey, path, false))
            return null;

        if (survey.ErrorsExist)
        {
            errorMessage = "Errors exist in survey definition. Can't unlock until definition is complete.";
            return null;
        }

        survey.SaveAlteredString = SurveyEngine.SurveyIdAlter(survey.Id);
        SurveyEngine.SurveyIdAlterTest(survey, survey.SaveAlteredString);

        return survey;
    }

    public static Survey? RelockSurvey(string path, out string errorMessage)
    {
        errorMessage = string.Empty;

        var survey = new Survey();
        if (!SurveyEngine.LoadSurvey(survey, path, false))
            return null;

        survey.SaveAlteredString = SurveyEngine.SurveyIdAlter(survey.Id);
        SurveyEngine.SurveyRelock(survey);

        return survey;
    }
}
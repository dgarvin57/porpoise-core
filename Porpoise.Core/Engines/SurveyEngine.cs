#nullable enable

using Porpoise.Core.DataAccess;
using Porpoise.Core.Extensions;
using Porpoise.Core.Models;
using Porpoise.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Porpoise.Core.Engines;

/// <summary>
/// The legendary SurveyEngine — core of all survey loading, saving, data handling,
/// template import, Orca import, and support/unlock features.
/// </summary>
public static class SurveyEngine
{
    public static void SaveCrossTabDataToCSV(List<CrosstabItem> list, string path)
        => LegacyDataAccess.ExportListToCSV(list, path);

    public static bool LoadSurvey(Survey survey, string surveyPath, bool exported)
    {
        if (survey == null) throw new ArgumentNullException(nameof(survey));
        if (string.IsNullOrWhiteSpace(surveyPath)) throw new ArgumentException("Survey path required.", nameof(surveyPath));

        var originalSurveyPath = survey.SurveyPath;
        var originalProjectFolder = survey.FullProjectFolder;

        survey = LegacyDataAccess.Read(surveyPath) ?? throw new InvalidOperationException("Failed to read survey file.");
        survey.SurveyPath = surveyPath;

        if (string.IsNullOrEmpty(survey.FullProjectFolder))
        {
            survey.FullProjectFolder = IOUtils.GetFullProjectPathFromSurveyPath(surveyPath, survey.SurveyFolder, survey.SurveyFileName);
        }

        if (!LoadSurveyData(survey, false, exported))
            return false;

        return true;
    }

    public static bool LoadSurveyData(Survey survey, bool useOriginalDataPath, bool exported)
    {
        if (survey == null) return false;
        if (string.IsNullOrEmpty(survey.FullProjectFolder)) return false;

        string dataPath = useOriginalDataPath
            ? survey.OrigDataFilePath
            : Path.Combine(survey.FullProjectFolder, survey.SurveyFolder, survey.DataFileName);

        if (string.IsNullOrEmpty(dataPath))
            throw new ArgumentNullException("Survey data file path is required.");

        List<List<string>>? data;

        try
        {
            if (Path.GetExtension(survey.DataFileName)?.Equals(".porpd", StringComparison.OrdinalIgnoreCase) == true)
            {
                var binaryPath = Path.Combine(survey.FullProjectFolder, survey.SurveyName,
                    Path.GetFileNameWithoutExtension(survey.DataFileName) + ".porpd");
                survey.DataFileName = Path.GetFileName(binaryPath);
                survey.Data.DataFilePath = binaryPath;
                data = LegacyDataAccess.ReadSurveyDataFromBinary(binaryPath);
            }
            else
            {
                data = LegacyDataAccess.ReadSurveyDataFromCSV(dataPath);
            }
        }
        catch (SurveyDataFileIsNotCSVTypeException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var message = string.IsNullOrEmpty(ex.Message)
                ? "Survey data file may be corrupted"
                : ex.Message;

            throw new SurveyDataFileLoadException(
                message, ex.InnerException, survey.SurveyPath, dataPath,
                survey.Id, survey.OrigDataFilePath, exported);
        }

        if (data == null) return false;

        survey.Data ??= new SurveyData();
        survey.Data.DataList = data;
        survey.Data.DataFilePath = dataPath;
        survey.DataFileName = Path.GetFileName(dataPath);

        if (!survey.Data.IsAllResponsesNumeric()) return false;
        if (!survey.Data.IsAllCasesInteger()) return false;
        if (!survey.IsAllResponsesInQuestionMissingValuesOK()) return false;

        return true;
    }

    public static bool ReplaceDataFile(string fullProjectPath, string surveyPath, Guid surveyId, string newFile, bool exported)
    {
        if (string.IsNullOrWhiteSpace(surveyPath)) throw new ArgumentNullException(nameof(surveyPath));
        if (surveyId == Guid.Empty) throw new ArgumentNullException(nameof(surveyId));
        if (string.IsNullOrWhiteSpace(newFile)) throw new ArgumentNullException(nameof(newFile));
        if (!File.Exists(newFile)) throw new FileNotFoundException("New data file not found.", newFile);
        if (!File.Exists(surveyPath)) throw new FileNotFoundException("Survey file not found.", surveyPath);

        var survey = LegacyDataAccess.Read(surveyPath) ?? throw new InvalidOperationException("Failed to load survey.");
        var originalDataPath = survey.Data.DataFilePath;

        survey.OrigDataFilePath = newFile;
        survey.FullProjectFolder = fullProjectPath;

        if (!LoadSurveyData(survey, true, false))
            return false;

        var backupPath = Path.Combine(Path.GetDirectoryName(originalDataPath)!,
            "Copy of " + Path.GetFileName(originalDataPath));
        File.Copy(originalDataPath, backupPath, true);
        File.Delete(originalDataPath);

        return SaveSurvey(survey, exported);
    }

    public static bool LoadDataIntoQuestions(Survey survey, string? orcaXmlPath)
    {
        if (survey.Data == null)
            throw new ArgumentNullException("Survey data is required.");

        survey.QuestionList ??= new ObjectListBase<Question>();
        survey.QuestionList.Clear();

        for (int i = 1; i < survey.Data.DataList[0].Count; i++)
        {
            if (survey.Data.DataList[0][i].Equals("WEIGHT", StringComparison.OrdinalIgnoreCase))
                continue;

            var question = new Question
            {
                DataFileCol = i,
                QstNumber = survey.Data.DataList[0][i],
                QstLabel = ""
            };

            if (survey.Data.MissingResponseValues.Count > 0) question.MissValue1 = survey.Data.MissingResponseValues[0].ToString();
            if (survey.Data.MissingResponseValues.Count > 1) question.MissValue2 = survey.Data.MissingResponseValues[1].ToString();
            if (survey.Data.MissingResponseValues.Count > 2) question.MissValue3 = survey.Data.MissingResponseValues[2].ToString();

            var responses = survey.Data.GetQuestionResponses(i, question.MissingValues);
            question.Responses = responses;

            question.DataType = responses.Count <= 12 ? QuestionDataType.Nominal : QuestionDataType.Interval;

            if (question.QstNumber.Contains("a"))
                question.BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock;
            else if (Regex.IsMatch(question.QstNumber, @"[0-9][a-zA-Z]", RegexOptions.IgnoreCase))
                question.BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion;
            else
                question.BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion;

            question.IsDirty = false;
            survey.QuestionList.Add(question);
        }

        if (string.IsNullOrEmpty(orcaXmlPath))
            return true;

        return ImportOrcaQuestionData(survey, orcaXmlPath);
    }

    private static bool ImportOrcaQuestionData(Survey survey, string xmlPath)
    {
        var variables = LegacyDataAccess.GetOrcaXMLQuestionFile(xmlPath);
        if (variables == null || variables.Count == 0) return false;

        foreach (var question in survey.QuestionList)
        {
            var orcaVar = variables.Find(v => v.QuestionNumber == question.QstNumber);
            if (orcaVar == null) continue;

            question.QstLabel = orcaVar.QuestionLabel;
            question.BlkQstStatus = orcaVar.BlockType;

            foreach (var resp in question.Responses)
            {
                var orcaResp = orcaVar.UniqueResponses.Find(r => r.RespValue == resp.RespValue);
                if (orcaResp != null)
                    resp.Label = orcaResp.Label;
            }
        }

        // Add missing responses from Orca
        foreach (var v in variables)
        {
            var porpoiseQuestion = survey.QuestionList.Find(q => q.QstNumber == v.QuestionNumber);
            if (porpoiseQuestion == null) continue;

            foreach (var orcaResp in v.UniqueResponses)
            {
                if (survey.Data.MissingResponseValues.Contains(orcaResp.RespValue)) continue;
                if (porpoiseQuestion.Responses.Any(r => r.RespValue == orcaResp.RespValue)) continue;

                porpoiseQuestion.Responses.Add(new Response
                {
                    RespValue = orcaResp.RespValue,
                    Label = orcaResp.Label
                });
            }
        }

        return true;
    }

    public static void LoadTemplateIntoQuestion(List<Question> template, string templateFileName, Survey survey, ref string message)
    {
        if (template == null || template.Count == 0 || survey?.QuestionList == null || survey.QuestionList.Count == 0)
        {
            message = $"Template '{templateFileName}' was not applied — no matching questions found.";
            return;
        }

        int matched = 0;

        foreach (var tq in template)
        {
            var existing = survey.QuestionList.FirstOrDefault(q => q.QstNumber == tq.QstNumber);
            if (existing == null) continue;

            matched++;
            existing.BlkLabel = tq.BlkLabel;
            existing.BlkStem = tq.BlkStem;
            existing.BlkQstStatus = tq.BlkQstStatus;
            existing.QstLabel = tq.QstLabel;
            existing.DataType = tq.DataType;
            existing.MissValue1 = tq.MissValue1;
            existing.MissValue2 = tq.MissValue2;
            existing.MissValue3 = tq.MissValue3;
            existing.QstStem = tq.QstStem;
            existing.QuestionNotes = tq.QuestionNotes ?? existing.QuestionNotes;
            existing.VariableType = tq.VariableType;

            foreach (var tr in tq.Responses)
            {
                var er = existing.Responses.FirstOrDefault(r => r.RespValue == tr.RespValue);
                if (er != null)
                {
                    er.IndexType = tr.IndexType;
                    er.Label = tr.Label;
                }
            }
        }

        int totalQuestions = survey.Data.DataList[0].Count - 1;
        message = matched > 0
            ? $"Template successfully applied. Matched {matched} of {totalQuestions} questions."
            : $"Template '{templateFileName}' was not applied — no matching questions found.";
    }

    public static bool SaveSurvey(Survey survey, bool exporting)
    {
        if (string.IsNullOrEmpty(survey.FullProjectFolder)) throw new ArgumentNullException("Project folder required.");
        if (string.IsNullOrEmpty(survey.SurveyName)) throw new ArgumentNullException("Survey name required.");
        if (string.IsNullOrEmpty(survey.DataFileName)) throw new ArgumentNullException("Data file name required.");

        survey.Data.RemoveWeightsFromDataList();
        survey.Data.RemoveMovementColumn();

        var surveyFolder = Path.Combine(survey.FullProjectFolder, survey.SurveyName);
        survey.SurveyFolder = survey.SurveyName;
        var dataPath = Path.Combine(surveyFolder, survey.DataFileName);
        survey.SurveyPath = Path.Combine(surveyFolder, survey.SurveyName + ".porps");
        survey.SurveyFileName = survey.SurveyName + ".porps";

        survey.ModifiedOn = DateTime.Now;
        survey.ModifiedBy = Environment.UserName;

        if (!IOUtils.CreateDirectory(surveyFolder)) return false;

        if (exporting)
        {
            var binaryName = Path.GetFileNameWithoutExtension(survey.DataFileName) + ".porpd";
            var binaryPath = Path.Combine(surveyFolder, binaryName);
            survey.DataFileName = binaryName;
            survey.Data.DataFilePath = binaryPath;
            LegacyDataAccess.WriteSurveyDataToBinary(survey.Data, binaryPath);

            var csvPath = Path.Combine(surveyFolder, Path.GetFileNameWithoutExtension(survey.DataFileName) + ".csv");
            if (File.Exists(csvPath)) File.Delete(csvPath);
        }
        else
        {
            survey.Data.DataFilePath = dataPath;
            if (!LegacyDataAccess.ExportDataTableToCSV(survey.Data.ToDataTable(true, false), dataPath))
                return false;
        }

        var result = LegacyDataAccess.Write(survey, survey.SurveyPath);
        if (result)
        {
            survey.MarkClean();
            survey.MarkAsOld();
        }

        return result;
    }

    public static string SurveyIdAlter(Guid surveyId) => LegacyDataAccess.SurveyIdMangle(surveyId);

    public static bool SurveyIdAlterTest(Survey survey, string tested)
    {
        if (tested == SurveyIdAlter(survey.Id))
        {
            survey.LockStatus = LockStatusType.Unlocked;
            survey.UnlockKeyName = "Manual Unlock";
            survey.UnlockKeyType = KeyType.Manual;
            LegacyDataAccess.Write(survey, survey.SurveyPath);
            return true;
        }
        return false;
    }

    public static bool SurveyRelock(Survey survey)
    {
        survey.LockStatus = LockStatusType.Locked;
        survey.UnlockKeyName = "";
        survey.UnlockKeyType = KeyType.None;
        LegacyDataAccess.Write(survey, survey.SurveyPath);
        return true;
    }

    public static string FormatSelectedResponsesToString(Question question)
    {
        if (question?.Responses == null || !question.Responses.Any())
            return "No selected responses";

        var sb = new StringBuilder("Selected responses:").AppendLine();
        foreach (var r in question.Responses)
            sb.AppendLine($" - {r.Label}");

        return sb.ToString();
    }

    public static string FormatSelectPlusSelectionToString(SurveyData data, bool shortMsg)
    {
        var sb = new StringBuilder();

        if (shortMsg)
        {
            var q1 = data.SelectPlusQ1?.QstLabel.Length > 5 ? data.SelectPlusQ1.QstLabel.Substring(0, 5) : data.SelectPlusQ1?.QstLabel ?? "N/A";
            var q2 = data.SelectPlusQ2?.QstLabel.Length > 5 ? data.SelectPlusQ2.QstLabel.Substring(0, 5) : data.SelectPlusQ2?.QstLabel ?? "N/A";
            var movement = data.SelectPlusCondition switch
            {
                SelectPlusConditionType.GoesPositive => "Go+",
                SelectPlusConditionType.GoesNegative => "Go-",
                SelectPlusConditionType.StaysPositive => "Stay+",
                SelectPlusConditionType.StaysNegative => "Stay-",
                SelectPlusConditionType.StaysNeutral => "Stay/",
                _ => "None"
            };
            sb.Append($"Select+: {q1} to {q2} ({movement})");
        }
        else
        {
            sb.AppendLine("Select Plus:");
            sb.Append(" - From Q1: ").AppendLine(data.SelectPlusQ1?.QstLabel ?? "N/A");
            sb.Append(" - To Q2: ").AppendLine(data.SelectPlusQ2?.QstLabel ?? "N/A");
            sb.Append(" - Movement: ").AppendLine(data.SelectPlusCondition.Description());
        }

        return sb.ToString();
    }

    public static string FormatWeightedResponsesToString(Question question)
    {
        if (question?.Responses == null || !question.Responses.Any(r => Math.Abs(r.Weight - 1.0) > 0.001))
            return "No weighted responses";

        var sb = new StringBuilder("Weighted responses:").AppendLine();
        foreach (var r in question.Responses.Where(r => Math.Abs(r.Weight - 1.0) > 0.001))
            sb.AppendLine($" - {r.Label}: {r.Weight:#.##0}");

        return sb.ToString();
    }

    public static OrcaExport? GetOrcaExportInterfaceFile(string path)
        => LegacyDataAccess.GetOrcaExportInterfaceFile(path);
}
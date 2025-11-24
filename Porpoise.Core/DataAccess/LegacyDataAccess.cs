// Porpoise.Core\DataAccess\Stubs\LegacyDataAccessStubs.cs
#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Porpoise.Core.Models;

namespace Porpoise.Core.DataAccess;

/// <summary>
/// Fully self-contained in-memory stubs for ALL old *Store static methods.
/// NO reference to Porpoise.DataAccess required.
/// Keeps your new Engines compiling and running today.
/// </summary>
public static class LegacyDataAccess
{
    private static readonly Dictionary<string, Survey> _surveys = new();
    private static readonly Dictionary<string, Project> _projects = new();
    private static readonly Dictionary<string, ErrorLogs> _errorLogs = new();

    // Survey
    public static Survey? ReadSurvey(string path) =>
        _surveys.TryGetValue(path, out var survey) ? survey : null;

    public static bool Read(ref Survey survey, string path)
    {
        if (_surveys.TryGetValue(path, out var loaded))
        {
            survey = loaded;
            return true;
        }
        return false;
    }

    public static bool Read(ref Project project, string path)
    {
        if (_projects.TryGetValue(path, out var loaded))
        {
            project = loaded;
            return true;
        }
        return false;
    }

    public static bool WriteSurvey(Survey survey, string path)
    {
        _surveys[path] = survey;
        return true;
    }

    // Project
    public static Project? ReadProject(string path) =>
        _projects.TryGetValue(path, out var project) ? project : null;

    public static bool WriteProject(Project project, string path)
    {
        _projects[path] = project;
        return true;
    }

    // These two are the ones that match the old Store.Write() calls
    public static bool Write(Survey survey, string path)
    {
        _surveys[path] = survey;
        return true;
    }

    public static bool Write(Project project, string path)
    {
        _projects[path] = project;
        return true;
    }

    // Survey Data (stubs — no real I/O)
    public static List<List<string>>? ReadSurveyDataFromBinary(string path) => null;
    public static List<List<string>>? ReadSurveyDataFromCSV(string path) => null;
    public static void WriteSurveyDataToBinary(SurveyData data, string path) { }

    // Error Logging
    public static void SaveErrorToXmlFile(string path, ErrorLog errorLog)
    {
        if (!_errorLogs.ContainsKey(path))
            _errorLogs[path] = new ErrorLogs();
        _errorLogs[path].Add(errorLog);
    }

    public static ErrorLogs GetAllErrorsFromXmlFile(string path) =>
        _errorLogs.TryGetValue(path, out var logs) ? logs : new ErrorLogs();

    public static void SaveAllErrorsToXmlFile(string path, ErrorLogs logs, int maxSize) =>
        _errorLogs[path] = logs;

    // Support / Unlock
    public static bool CheckSupportOk(string input) => true;
    public static string TranslateOne(string input) => input;
    public static string TranslateBack(string input) => input;
    public static string SurveyIdMangle(Guid id) => id.ToString("N");

    // LegacyDataAccess
    public static bool ExportListToCSV<T>(List<T> list, string path) => true;
    public static bool ExportDataTableToCSV(System.Data.DataTable dt, string path) => true;

    public static List<T>? LoadXMLFileToList<T>(string path) => null;

    public static bool SaveListToXMLFile<T>(List<T> list, string path)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(List<T>));
            using var writer = new StreamWriter(path);
            serializer.Serialize(writer, list);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Orca (if used)
    public static List<OrcaVariableDef>? GetOrcaXMLQuestionFile(string path) => null;
    public static OrcaExport? GetOrcaExportInterfaceFile(string path) => null;
}
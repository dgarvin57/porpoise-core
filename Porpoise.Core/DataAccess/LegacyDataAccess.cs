// Porpoise.Core\DataAccess\Stubs\LegacyDataAccessStubs.cs
#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
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
    private static readonly Dictionary<string, Survey> _surveys = [];
    private static readonly Dictionary<string, Project> _projects = [];
    private static readonly Dictionary<string, ErrorLogs> _errorLogs = [];

    // Survey - Modern return pattern instead of ref parameters
    public static Survey? ReadSurvey(string path) =>
        _surveys.TryGetValue(path, out var survey) ? survey : null;

    public static Survey? Read(Survey? survey, string path)
    {
        if (_surveys.TryGetValue(path, out var loaded))
        {
            return loaded;
        }
        return survey;
    }

    public static Project? Read(Project? project, string path)
    {
        if (_projects.TryGetValue(path, out var loaded))
        {
            return loaded;
        }
        return project;
    }

    public static bool WriteSurvey(Survey survey, string path)
    {
        ArgumentNullException.ThrowIfNull(survey);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        
        _surveys[path] = survey;
        return true;
    }

    // Project
    public static Project? ReadProject(string path) =>
        _projects.TryGetValue(path, out var project) ? project : null;

    public static bool WriteProject(Project project, string path)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        
        _projects[path] = project;
        return true;
    }

    // These two are the ones that match the old Store.Write() calls
    public static bool Write(Survey survey, string path)
    {
        ArgumentNullException.ThrowIfNull(survey);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        
        _surveys[path] = survey;
        return true;
    }

    public static bool Write(Project project, string path)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        
        _projects[path] = project;
        return true;
    }

    // Survey Data (stubs — no real I/O)
    public static List<List<string>>? ReadSurveyDataFromBinary(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return null;
    }

    public static List<List<string>>? ReadSurveyDataFromCSV(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return null;
    }

    public static void WriteSurveyDataToBinary(SurveyData data, string path)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        // Stub - no implementation
    }

    // Error Logging
    public static void SaveErrorToXmlFile(string path, ErrorLog errorLog)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(errorLog);
        
        if (!_errorLogs.ContainsKey(path))
            _errorLogs[path] = [];
        _errorLogs[path].Add(errorLog);
    }

    public static ErrorLogs GetAllErrorsFromXmlFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return _errorLogs.TryGetValue(path, out var logs) ? logs : [];
    }

    public static void SaveAllErrorsToXmlFile(string path, ErrorLogs logs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(logs);
        
        _errorLogs[path] = logs;
    }

    // Support / Unlock
    public static bool CheckSupportOk(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        return true;
    }

    public static string TranslateOne(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        return input;
    }

    public static string TranslateBack(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        return input;
    }

    public static string SurveyIdMangle(Guid id) => id.ToString("N");

    // Export
    public static bool ExportListToCSV<T>(List<T> list, string path)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return true;
    }

    public static bool ExportDataTableToCSV(System.Data.DataTable dt, string path)
    {
        ArgumentNullException.ThrowIfNull(dt);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return true;
    }

    public static List<T>? LoadXMLFileToList<T>(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return null;
    }

    public static bool SaveListToXMLFile<T>(List<T> list, string path)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        
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
    public static List<OrcaVariableDef>? GetOrcaXMLQuestionFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return null;
    }

    public static OrcaExport? GetOrcaExportInterfaceFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return null;
    }

    // Support Engine methods
    public static Survey? ReadProjectSurveyFile(string path) => ReadSurvey(path);
    
    public static List<List<string>>? ReadDataFile(string path) => ReadSurveyDataFromCSV(path);
}
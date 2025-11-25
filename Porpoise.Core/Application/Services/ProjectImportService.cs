// Porpoise.Core/Application/Services/ProjectImportService.cs
#nullable enable

using Porpoise.Core.DataAccess.Legacy; // for SurveyStore.Read, etc.
using Porpoise.Core.Models;
using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Serialization;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Read-only importer for legacy .porp project files
/// Supports opening 20-year-old Porpoise projects — but never writes them
/// </summary>
public class ProjectImportService
{
    private readonly string _tempExtractPath;

    public ProjectImportService()
    {
        _tempExtractPath = Path.Combine(Path.GetTempPath(), "PorpoiseImport", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempExtractPath);
    }

    public Project? ImportPorpFile(string porpPath)
    {
        if (!File.Exists(porpPath))
            throw new FileNotFoundException($"Project file not found: {porpPath}");

        try
        {
            // Extract .porp (it's just a ZIP)
            ZipFile.ExtractToDirectory(porpPath, _tempExtractPath, overwriteFiles: true);

            // Find the .porp project file inside
            var projectFile = Directory.GetFiles(_tempExtractPath, "*.porp", SearchOption.AllDirectories)
                .FirstOrDefault() ?? throw new InvalidOperationException("No .porp project file found inside archive.");

            // Load the Project object (old format)
            var project = LegacyDataAccess.ReadProject(projectFile);
            if (project == null)
                throw new InvalidDataException("Failed to deserialize project file.");

            // Load all surveys
            foreach (var summary in project.SurveyListSummary)
            {
                var surveyPath = Path.Combine(_tempExtractPath, summary.SurveyFolder, summary.SurveyFileName);
                var survey = new Survey();
                LegacyDataAccess.Read(ref survey, surveyPath);

                // Load survey data (binary .porpd or CSV)
                var dataPath = Path.Combine(_tempExtractPath, summary.SurveyFolder,
                    Path.GetFileNameWithoutExtension(summary.SurveyFileName) + ".porpd");

                if (File.Exists(dataPath))
                {
                    survey.Data.DataList = LegacyDataAccess.ReadSurveyDataFromBinary(dataPath);
                }
                else
                {
                    var csvPath = dataPath.Replace(".porpd", ".csv");
                    if (File.Exists(csvPath))
                        survey.Data.DataList = LegacyDataAccess.ReadSurveyDataFromCSV(csvPath);
                }

                project.SurveyList.Add(survey);
            }

            project.FullFolder = _tempExtractPath;
            project.IsExported = true;

            return project;
        }
        catch
        {
            Cleanup();
            throw;
        }
    }

    private void Cleanup()
    {
        try
        {
            if (Directory.Exists(_tempExtractPath))
                Directory.Delete(_tempExtractPath, recursive: true);
        }
        catch { /* best effort */ }
    }

    public void Dispose() => Cleanup();
}
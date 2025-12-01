using Porpoise.Core.Data;
using Porpoise.Core.Models;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Porpoise.Core.Application.Services;

/// <summary>
/// Imports legacy .porpz (zipped exported project) files from Porpoise Desktop.
/// A .porpz file is a ZIP archive containing:
/// - One .porp project file
/// - Multiple survey folders, each containing:
///   - .porps survey definition file
///   - .porpd binary data file
/// </summary>
public class PorpzImportService : IDisposable
{
    private string? _tempExtractPath;
    private bool _disposed;

    public class ImportResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Project? Project { get; set; }
        public string? ProjectPath { get; set; }
    }

    /// <summary>
    /// Imports a .porpz file into the specified destination directory.
    /// </summary>
    /// <param name="porpzFilePath">Path to the .porpz file to import</param>
    /// <param name="destinationBaseDir">Base directory where projects are stored</param>
    /// <returns>Import result with project or error message</returns>
    public ImportResult ImportPorpzFile(string porpzFilePath, string destinationBaseDir)
    {
        var result = new ImportResult();

        try
        {
            // Validate input
            if (!File.Exists(porpzFilePath))
            {
                result.ErrorMessage = $"The file '{porpzFilePath}' does not exist.";
                return result;
            }

            if (Path.GetExtension(porpzFilePath).ToLowerInvariant() != ".porpz")
            {
                result.ErrorMessage = $"Only .porpz files can be imported into Porpoise.{Environment.NewLine}{Environment.NewLine}" +
                                    $"The file you selected '{Path.GetFileName(porpzFilePath)}' has the wrong file extension.";
                return result;
            }

            if (!Directory.Exists(destinationBaseDir))
            {
                result.ErrorMessage = $"The destination directory '{destinationBaseDir}' does not exist.";
                return result;
            }

            // Extract to temp directory first
            _tempExtractPath = Path.Combine(Path.GetTempPath(), "PorpoiseZip", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempExtractPath);

            // Find and extract project file
            string? projectFileName = null;
            using (var archive = ZipFile.OpenRead(porpzFilePath))
            {
                var porpFiles = archive.Entries.Where(e => Path.GetExtension(e.Name).ToLowerInvariant() == ".porp").ToList();

                if (porpFiles.Count == 0)
                {
                    result.ErrorMessage = $"Project export file '{Path.GetFileName(porpzFilePath)}' cannot be imported because it is corrupted.{Environment.NewLine}{Environment.NewLine}" +
                                        "Sorry, but a valid project file (.porp) was not found in the exported bundle of files, which indicates the export was flawed.{Environment.NewLine}{Environment.NewLine}" +
                                        "Try having the sender re-export the project and try to import it again. Or, contact Porpoise support for assistance.";
                    return result;
                }

                // If multiple .porp files, try to match the zip filename
                if (porpFiles.Count > 1)
                {
                    var zipBaseName = Path.GetFileNameWithoutExtension(porpzFilePath);
                    var matchingFile = porpFiles.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f.Name) == zipBaseName);

                    if (matchingFile == null)
                    {
                        result.ErrorMessage = $"Project export file '{Path.GetFileName(porpzFilePath)}' cannot be imported because it is corrupted.{Environment.NewLine}{Environment.NewLine}" +
                                            "Sorry, but multiple project files were found in the export and none of them match the name of the export file; so not sure which one to use.{Environment.NewLine}{Environment.NewLine}" +
                                            "Try having the sender re-export the project and try to import it again. Or, contact Porpoise support for assistance.";
                        return result;
                    }

                    projectFileName = matchingFile.FullName;
                }
                else
                {
                    projectFileName = porpFiles[0].FullName;
                }

                // Extract the project file
                var projectEntry = archive.GetEntry(projectFileName);
                if (projectEntry == null)
                {
                    result.ErrorMessage = "Failed to extract project file from archive.";
                    return result;
                }

                var tempProjectPath = Path.Combine(_tempExtractPath, projectFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(tempProjectPath)!);
                projectEntry.ExtractToFile(tempProjectPath, true);
            }

            // Load the project to get its name
            var project = new Project();
            LegacyDataAccess.Read(ref project, Path.Combine(_tempExtractPath, projectFileName));

            if (string.IsNullOrEmpty(project.ProjectName))
            {
                result.ErrorMessage = "Project file is invalid or corrupted - missing project name.";
                return result;
            }

            // Determine destination path
            var destinationProjectFolder = Path.Combine(destinationBaseDir, project.ProjectName);
            var destinationProjectPath = Path.Combine(destinationProjectFolder, project.FileName ?? $"{project.ProjectName}.porp");

            // Check if destination exists and is not exported
            if (File.Exists(destinationProjectPath))
            {
                var existingProject = new Project();
                try
                {
                    LegacyDataAccess.Read(ref existingProject, destinationProjectPath);

                    if (!existingProject.IsExported)
                    {
                        result.ErrorMessage = $"Project '{project.ProjectName}' cannot be imported because a project of the same name already exists.{Environment.NewLine}{Environment.NewLine}" +
                                            "This message is displayed when you are attempting to overwrite an active project (not one that has been imported) on your machine.{Environment.NewLine}{Environment.NewLine}" +
                                            $"Either rename or remove the project at '{destinationProjectFolder}' and then try importing again.";
                        return result;
                    }
                }
                catch
                {
                    // If we can't read existing project, treat as error
                    result.ErrorMessage = $"Cannot import project '{project.ProjectName}' - a project with that name already exists and cannot be validated.";
                    return result;
                }
            }

            // Extract entire archive to destination
            if (Directory.Exists(destinationProjectFolder))
            {
                // Delete existing exported project
                Directory.Delete(destinationProjectFolder, true);
            }

            using (var archive = ZipFile.OpenRead(porpzFilePath))
            {
                archive.ExtractToDirectory(destinationProjectFolder, true);
            }

            // Update project properties
            project.ProjectFolder = project.ProjectName;
            project.FullFolder = destinationProjectFolder;
            project.FullPath = destinationProjectPath;
            project.BaseProjectFolder = destinationBaseDir;
            project.IsExported = true;

            // Load all surveys
            if (project.SurveyListSummary != null)
            {
                project.SurveyList = new ObjectListBase<Survey>();

                foreach (var summary in project.SurveyListSummary)
                {
                    var survey = new Survey();
                    var surveyPath = Path.Combine(destinationProjectFolder, summary.SurveyFolder, summary.SurveyFileName);

                    if (File.Exists(surveyPath))
                    {
                        LegacyDataAccess.Read(ref survey, surveyPath);
                        survey.FullProjectFolder = destinationProjectFolder;
                        survey.SurveyPath = surveyPath;

                        // Clear any metadata-based data list from .porps file
                        // The .porps file contains survey metadata, not actual response data
                        if (survey.Data?.DataList != null && survey.Data.DataList.Count > 0)
                        {
                            // Check if first row contains metadata fields (old format)
                            var firstRow = survey.Data.DataList[0];
                            if (firstRow.Contains("CreatedOn") || firstRow.Contains("CreatedBy") || 
                                firstRow.Contains("ModifiedOn") || firstRow.Contains("IsDirty"))
                            {
                                // This is metadata, not survey responses - clear it
                                survey.Data.DataList = new List<List<string>>();
                            }
                        }

                        // Load binary data if exists
                        var dataFileName = Path.GetFileNameWithoutExtension(summary.SurveyFileName) + ".porpd";
                        var dataPath = Path.Combine(destinationProjectFolder, summary.SurveyFolder, dataFileName);

                        if (File.Exists(dataPath))
                        {
                            var dataList = LegacyDataAccess.ReadSurveyDataFromBinary(dataPath);
                            if (dataList != null && dataList.Count > 0)
                            {
                                survey.Data.DataList = dataList;
                                survey.Data.DataFilePath = dataPath;
                                survey.DataFileName = dataFileName;
                            }
                        }

                        project.SurveyList.Add(survey);
                    }
                }
            }

            result.Success = true;
            result.Project = project;
            result.ProjectPath = destinationProjectPath;

            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"An error occurred while importing the project:{Environment.NewLine}{Environment.NewLine}{ex.Message}";
            return result;
        }
        finally
        {
            Cleanup();
        }
    }

    private void Cleanup()
    {
        if (!string.IsNullOrEmpty(_tempExtractPath) && Directory.Exists(_tempExtractPath))
        {
            try
            {
                Directory.Delete(_tempExtractPath, true);
            }
            catch
            {
                // Ignore cleanup errors
            }

            _tempExtractPath = null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        Cleanup();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~PorpzImportService()
    {
        Dispose();
    }
}

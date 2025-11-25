#nullable enable

using Porpoise.Core.DataAccess;
using Porpoise.Core.Models;
using Porpoise.Core.Utilities;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Porpoise.Core.Engines;

/// <summary>
/// Core project-level operations: save, load, export, import, name validation.
/// This is the "Project" brain — everything project-wide flows through here.
/// </summary>
public static class ProjectEngine
{
    public static bool IsSurveyNameUnique(Project project, Survey thisSurvey, string name)
    {
        return project.SurveyList?.Any(s => s.SurveyName == name && !ReferenceEquals(s, thisSurvey)) != true;
    }

    public static bool IsProjectNameUnique(string fullFolder, string baseProjectFolder, string name)
    {
        var basePath = IOUtils.GetFullBaseProjectPath(fullFolder ?? string.Empty, baseProjectFolder ?? string.Empty);
        var path = Path.Combine(basePath ?? string.Empty, name);
        return !Directory.Exists(path);
    }

    public static bool SaveProject(Project project, string path)
    {
        GetProjectFileProperties(project, path);

        IOUtils.CreateDirectory(project.FullFolder ?? string.Empty);

        project.ModifiedBy = Environment.UserName;
        project.ModifiedOn = DateTime.Now;

        if (project.IsNew)
        {
            project.MarkAsOld();
        }

        if (project.SurveyList?.Any() == true)
        {
            foreach (var survey in project.SurveyList)
            {
                survey.FullProjectFolder = project.FullFolder ?? string.Empty;
                SurveyEngine.SaveSurvey(survey, project.IsExported);
            }

            project.SummarizeSurveyList();
        }

        // Save researcher logo as PNG if present
        if (project.ResearcherLogo != null && project.FullFolder != null)
        {
            var logoPath = Path.Combine(project.FullFolder, project.ResearcherLogoFilename ?? "logo.png");
            if (!File.Exists(logoPath))
            {
                var pngName = Path.GetFileNameWithoutExtension(project.ResearcherLogoFilename ?? "logo") + ".png";
                var pngPath = Path.Combine(project.FullFolder, pngName);
                project.ResearcherLogo.Save(pngPath, System.Drawing.Imaging.ImageFormat.Png);
                project.ResearcherLogoFilename = pngName;
            }
        }

        bool success = LegacyDataAccess.Write(project, path);

        if (success)
        {
            project.MarkClean();
            project.MarkAsOld();
        }

        return success;
    }

    public static Project LoadProjectFromFile(Project project, string path, string baseProjectFolder)
    {
        project = LegacyDataAccess.Read(project, path) ?? project;
        project.BaseProjectFolder = baseProjectFolder;
        GetProjectFileProperties(project, path);

        if (project.SurveyListSummary?.Any() == true && project.FullFolder != null)
        {
            var surveyList = new ObjectListBase<Survey>();

            foreach (var summary in project.SurveyListSummary)
            {
                var surveyPath = Path.Combine(project.FullFolder, summary.SurveyFolder ?? string.Empty, summary.SurveyFileName ?? string.Empty);
                var survey = new Survey
                {
                    FullProjectFolder = project.FullFolder,
                    SurveyPath = surveyPath
                };

                SurveyEngine.LoadSurvey(survey, surveyPath, project.IsExported);
                surveyList.Add(survey);
            }

            project.SurveyList = surveyList;
        }

        // Load researcher logo
        if (!string.IsNullOrEmpty(project.ResearcherLogoFilename) && project.FullFolder != null)
        {
            var imagePath = Path.Combine(project.FullFolder, project.ResearcherLogoFilename);
            if (File.Exists(imagePath))
            {
                project.ResearcherLogo = IOUtils.GetImageUsingFileStream(imagePath);
            }
        }

        project.MarkClean();
        project.MarkAsOld();

        return project;
    }

    public static void GetProjectFileProperties(Project project, string path)
    {
        project.FileName = Path.GetFileName(path);
        project.FullPath = Path.GetFullPath(path);
        project.FullFolder = Path.GetDirectoryName(path)!;
        project.ProjectFolder = IOUtils.GetProjectFolderDirectoryPart(path, project.BaseProjectFolder ?? string.Empty) ?? string.Empty;
    }

    public static void ExportProject(string projectFilename, string baseProjectFolder,
        string currentProjectFullFolder, string exportPath, bool removeNotes)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(projectFilename));
        IOUtils.CopyDirectory(currentProjectFullFolder, tempDir);

        var tempProjectPath = Path.Combine(tempDir, projectFilename);
        var project = new Project();
        LoadProjectFromFile(project, tempProjectPath, baseProjectFolder);
        project.ProjectFolder = IOUtils.GetLastPartOfFolderPath(project.FullFolder ?? string.Empty) ?? string.Empty;
        project.IsExported = true;

        if (removeNotes && project.SurveyList != null)
        {
            foreach (var survey in project.SurveyList)
            {
                survey.SurveyNotes = "";
                foreach (var question in survey.QuestionList)
                    question.QuestionNotes = "";
            }
        }

        SaveProject(project, tempProjectPath);

        // Use System.IO.Compression to create zip from directory
        ZipFile.CreateFromDirectory(tempDir, exportPath, CompressionLevel.Optimal, false);

        IOUtils.DeleteDirectory(tempDir, true);
    }

    public static string? ImportProject(string zipFilePath, string destinationDir, out string errorMessage)
    {
        errorMessage = string.Empty;
        var tempDir = Path.Combine(Path.GetTempPath(), "PorpoiseZip");
        string? extractedProjectFile = null;

        try
        {
            using var archive = ZipFile.OpenRead(zipFilePath);
            var porpEntries = archive.Entries.Where(e => Path.GetExtension(e.FullName) == ".porp").ToList();

            if (porpEntries.Count == 0)
            {
                errorMessage = $"Project export file '{Path.GetFileName(zipFilePath)}' is corrupted — no .porp file found.";
                return null;
            }

            // Prefer .porp file that matches zip filename
            extractedProjectFile = porpEntries
                .FirstOrDefault(e => Path.GetFileNameWithoutExtension(e.FullName) ==
                                    Path.GetFileNameWithoutExtension(zipFilePath))?.FullName
                ?? porpEntries.First().FullName;

            // Extract just the project file first to read its name
            var entry = archive.Entries.First(e => e.FullName == extractedProjectFile);

            // Ensure directory exists and extract single entry
            Directory.CreateDirectory(tempDir);
            var extractedPath = Path.Combine(tempDir, entry.FullName);
            Directory.CreateDirectory(Path.GetDirectoryName(extractedPath) ?? tempDir);
            entry.ExtractToFile(extractedPath, true);

            var tempProjectPath = Path.Combine(tempDir, extractedProjectFile);
            var project = new Project();
            project = LegacyDataAccess.Read(project, tempProjectPath) ?? project;
            GetProjectFileProperties(project, tempProjectPath);

            var targetFolder = Path.Combine(destinationDir, project.ProjectName ?? "ImportedProject");
            var targetProjectPath = Path.Combine(targetFolder, project.FileName ?? "project.porp");

            if (File.Exists(targetProjectPath))
            {
                var existing = new Project();
                existing = LegacyDataAccess.Read(existing, targetProjectPath) ?? existing;
                if (!existing.IsExported)
                {
                    errorMessage = $"Cannot import: A project named '{project.ProjectName}' already exists and is not exported.";
                    return null;
                }
            }

            // Extract all files to target folder (overwrite existing)
            ZipFile.ExtractToDirectory(zipFilePath, targetFolder, true);
            return targetProjectPath;
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    public static bool UpdateSurvey(Project project, Survey survey)
    {
        if (project.SurveyList == null) return false;

        for (int i = 0; i < project.SurveyList.Count; i++)
        {
            if (project.SurveyList[i] != null && project.SurveyList[i].Id == survey.Id)
            {
                project.SurveyList[i] = survey;
                return true;
            }
        }
        return false;
    }
}
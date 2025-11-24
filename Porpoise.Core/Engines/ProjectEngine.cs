#nullable enable

using Ionic.Zip;
using Porpoise.Core.DataAccess;
using Porpoise.Core.Model;
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
        return !project.SurveyList.Any(s => s.SurveyName == name && !ReferenceEquals(s, thisSurvey));
    }

    public static bool IsProjectNameUnique(string fullFolder, string baseProjectFolder, string name)
    {
        var path = Path.Combine(IOUtils.GetFullBaseProjectPath(fullFolder, baseProjectFolder), name);
        return !Directory.Exists(path);
    }

    public static bool SaveProject(Project project, string path)
    {
        GetProjectFileProperties(project, path);

        IOUtils.CreateDirectory(project.FullFolder);

        project.ModifiedBy = Environment.UserName;
        project.ModifiedOn = DateTime.Now;

        bool success = LegacyDataAccess.Write(project, path);

        if (project.IsNew)
        {
            project.MarkAsOld();
        }

        if (project.SurveyList.Any())
        {
            foreach (var survey in project.SurveyList)
            {
                survey.FullProjectFolder = project.FullFolder;
                SurveyEngine.SaveSurvey(survey, project.IsExported);
            }

            project.SummarizeSurveyList();
        }

        // Save researcher logo as PNG if present
        if (project.ResearcherLogo != null)
        {
            var logoPath = Path.Combine(project.FullFolder, project.ResearcherLogoFilename);
            if (!File.Exists(logoPath))
            {
                var pngName = Path.GetFileNameWithoutExtension(project.ResearcherLogoFilename) + ".png";
                var pngPath = Path.Combine(project.FullFolder, pngName);
                project.ResearcherLogo.Save(pngPath, System.Drawing.Imaging.ImageFormat.Png);
                project.ResearcherLogoFilename = pngName;
            }
        }

        success = LegacyDataAccess.Write(project, path);

        if (success)
        {
            project.MarkClean();
            project.MarkAsOld();
        }

        return success;
    }

    public static Project LoadProjectFromFile(Project project, string path, string baseProjectFolder)
    {
        LegacyDataAccess.Read(project, path);
        project.BaseProjectFolder = baseProjectFolder;
        GetProjectFileProperties(project, path);

        if (project.SurveyListSummary.Any())
        {
            var surveyList = new ObjectListBase<Survey>();

            foreach (var summary in project.SurveyListSummary)
            {
                var surveyPath = Path.Combine(project.FullFolder, summary.SurveyFolder, summary.SurveyFileName);
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
        if (!string.IsNullOrEmpty(project.ResearcherLogoFilename))
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
        project.ProjectFolder = IOUtils.GetProjectFolderDirectoryPart(path, project.BaseProjectFolder);
    }

    public static void ExportProject(string projectFilename, string baseProjectFolder,
        string currentProjectFullFolder, string exportPath, bool removeNotes)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(projectFilename));
        IOUtils.CopyDirectory(currentProjectFullFolder, tempDir);

        var tempProjectPath = Path.Combine(tempDir, projectFilename);
        var project = new Project();
        LoadProjectFromFile(project, tempProjectPath, baseProjectFolder);
        project.ProjectFolder = IOUtils.GetLastPartOfFolderPath(project.FullFolder);
        project.IsExported = true;

        if (removeNotes)
        {
            foreach (var survey in project.SurveyList)
            {
                survey.SurveyNotes = "";
                foreach (var question in survey.QuestionList)
                    question.QuestionNotes = "";
            }
        }

        SaveProject(project, tempProjectPath);

        using var zip = new ZipFile();
        zip.AddDirectory(tempDir);
        zip.Save(exportPath);

        IOUtils.DeleteDirectory(tempDir, true);
    }

    public static string? ImportProject(string zipFilePath, string destinationDir, out string errorMessage)
    {
        errorMessage = string.Empty;
        var tempDir = Path.Combine(Path.GetTempPath(), "PorpoiseZip");
        string? extractedProjectFile = null;

        try
        {
            using var zip = ZipFile.Read(zipFilePath);
            var porpEntries = zip.Entries.Where(e => Path.GetExtension(e.FileName) == ".porp").ToList();

            if (!porpEntries.Any())
            {
                errorMessage = $"Project export file '{Path.GetFileName(zipFilePath)}' is corrupted — no .porp file found.";
                return null;
            }

            // Prefer .porp file that matches zip filename
            extractedProjectFile = porpEntries
                .FirstOrDefault(e => Path.GetFileNameWithoutExtension(e.FileName) ==
                                    Path.GetFileNameWithoutExtension(zipFilePath))?.FileName
                ?? porpEntries.First().FileName;

            // Extract just the project file first to read its name
            var entry = zip.Entries.First(e => e.FileName == extractedProjectFile);
            entry.Extract(tempDir, ExtractExistingFileAction.OverwriteSilently);

            var tempProjectPath = Path.Combine(tempDir, extractedProjectFile);
            var project = new Project();
            LegacyDataAccess.Read(project, tempProjectPath);
            GetProjectFileProperties(project, tempProjectPath);

            var targetFolder = Path.Combine(destinationDir, project.ProjectName);
            var targetProjectPath = Path.Combine(targetFolder, project.FileName);

            if (File.Exists(targetProjectPath))
            {
                var existing = new Project();
                LegacyDataAccess.Read(existing, targetProjectPath);
                if (!existing.IsExported)
                {
                    errorMessage = $"Cannot import: A project named '{project.ProjectName}' already exists and is not exported.";
                    return null;
                }
            }

            zip.ExtractAll(targetFolder, ExtractExistingFileAction.OverwriteSilently);
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
        for (int i = 0; i < project.SurveyList.Count; i++)
        {
            if (project.SurveyList[i].Id == survey.Id)
            {
                project.SurveyList[i] = survey;
                return true;
            }
        }
        return false;
    }
}
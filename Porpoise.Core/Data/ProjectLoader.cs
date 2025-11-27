// Porpoise.Core/Data/ProjectLoader.cs
using System;
using System.IO;
using Porpoise.Core.Models;
using Porpoise.Core.DataAccess;

namespace Porpoise.Core.Data;

public static class ProjectLoader
{
    /// <summary>
    /// Loads a complete Porpoise project including survey definition, project metadata, and data
    /// </summary>
    /// <param name="surveyPath">Path to the .porps file</param>
    /// <param name="projectPath">Path to the .porp file</param>
    /// <param name="dataPath">Path to the .porpd file</param>
    /// <returns>Tuple containing Survey, Project, and SurveyData</returns>
    public static (Survey Survey, Project Project, SurveyData Data) LoadProject(string surveyPath, string projectPath, string dataPath)
    {
        if (!File.Exists(surveyPath))
            throw new FileNotFoundException($"Survey file not found: {surveyPath}");
        if (!File.Exists(projectPath))
            throw new FileNotFoundException($"Project file not found: {projectPath}");
        if (!File.Exists(dataPath))
            throw new FileNotFoundException($"Data file not found: {dataPath}");

        var survey = PorpoiseFileEncryption.ReadSurvey(surveyPath);
        var project = PorpoiseFileEncryption.ReadProject(projectPath);
        var data = PorpoiseFileEncryption.ReadData(dataPath);
        
        return (survey, project, data);
    }

    /// <summary>
    /// Loads only the survey definition from a .porps file
    /// </summary>
    /// <param name="surveyPath">Path to the survey file</param>
    /// <returns>Deserialized Survey object</returns>
    public static Survey LoadSurvey(string surveyPath)
    {
        if (!File.Exists(surveyPath))
            throw new FileNotFoundException($"Survey file not found: {surveyPath}");
        
        return PorpoiseFileEncryption.ReadSurvey(surveyPath);
    }
}
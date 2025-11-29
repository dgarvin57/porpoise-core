using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Data;
using Porpoise.Core.Models;
using Porpoise.Core.Services;

namespace Porpoise.Api.Controllers;

[ApiController]
[Route("api/survey-import")]
public class SurveyImportController : ControllerBase
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly SurveyPersistenceService? _persistenceService;
    private readonly IWebHostEnvironment _environment;

    public SurveyImportController(
        ISurveyRepository surveyRepository,
        IWebHostEnvironment environment,
        SurveyPersistenceService? persistenceService = null)
    {
        _surveyRepository = surveyRepository;
        _environment = environment;
        _persistenceService = persistenceService;
    }

    /// <summary>
    /// Helper method to import surveys from a directory containing .porp, .porps, and .porpd files
    /// </summary>
    private async Task<(List<object> ImportedSurveys, List<string> Errors, List<string> ReferencedFiles)> ImportSurveysFromDirectory(string directory, string projectFilePath)
    {
        var importedSurveys = new List<object>();
        var errors = new List<string>();
        var referencedFiles = new List<string>();

        // Load the project to get survey references
        var project = ProjectLoader.LoadProjectOnly(projectFilePath);
        
        if (project.SurveyListSummary == null || project.SurveyListSummary.Count == 0)
        {
            errors.Add("Project file contains no survey references");
            return (importedSurveys, errors, referencedFiles);
        }

        // List all referenced survey/data filenames
        foreach (var surveySummary in project.SurveyListSummary)
        {
            referencedFiles.Add($"Survey: {surveySummary.SurveyFileName}");
            referencedFiles.Add($"Data: {surveySummary.SurveyFileName.Replace(".porps", ".porpd")}");
        }

        // Import each survey referenced in the project
        foreach (var surveySummary in project.SurveyListSummary)
        {
            try
            {
                var projectFolder = Path.GetDirectoryName(projectFilePath) ?? directory;
                string? expectedSurveyPath = null;
                if (!string.IsNullOrEmpty(surveySummary.SurveyFolder))
                {
                    if (Path.IsPathRooted(surveySummary.SurveyFolder))
                    {
                        expectedSurveyPath = Path.Combine(surveySummary.SurveyFolder, surveySummary.SurveyFileName);
                    }
                    else
                    {
                        expectedSurveyPath = Path.Combine(projectFolder, surveySummary.SurveyFolder, surveySummary.SurveyFileName);
                    }
                }
                else
                {
                    expectedSurveyPath = Path.Combine(projectFolder, surveySummary.SurveyFileName);
                }

                string fallbackSurveyPath = Path.Combine(projectFolder, surveySummary.SurveyFileName);
                string surveyPath = System.IO.File.Exists(expectedSurveyPath) ? expectedSurveyPath : (System.IO.File.Exists(fallbackSurveyPath) ? fallbackSurveyPath : expectedSurveyPath);
                var dataPath = surveyPath.Replace(".porps", ".porpd");

                if (!System.IO.File.Exists(surveyPath))
                {
                    errors.Add($"Survey file not found: {surveySummary.SurveyFileName}\nTried: {expectedSurveyPath}\nAlso tried: {fallbackSurveyPath}");
                    continue;
                }

                // Load the survey
                var (survey, _, data) = ProjectLoader.LoadProject(
                    surveyPath,
                    projectFilePath,
                    System.IO.File.Exists(dataPath) ? dataPath : surveyPath
                );

                survey.Data = data;

                // Save using persistence service if available, otherwise use basic repository
                Survey savedSurvey;
                if (_persistenceService != null)
                {
                    savedSurvey = await _persistenceService.SaveSurveyWithDetailsAsync(survey, project);
                }
                else
                {
                    savedSurvey = await _surveyRepository.AddAsync(survey);
                }

                importedSurveys.Add(new
                {
                    SurveyId = savedSurvey.Id,
                    SurveyName = savedSurvey.SurveyName,
                    QuestionCount = savedSurvey.QuestionList?.Count ?? 0,
                    ResponseCount = data?.DataList?.Count - 1 ?? 0
                });
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing {surveySummary.SurveyFileName}: {ex.Message}");
            }
        }

        return (importedSurveys, errors, referencedFiles);
    }

    /// <summary>
    /// Import a .porps/.porpd file pair
    /// </summary>
    [HttpPost("porps")]
    public async Task<IActionResult> ImportPorpsFile(
        [FromForm] IFormFile surveyFile,
        [FromForm] IFormFile? dataFile,
        [FromForm] string? projectPath)
    {
        try
        {
            if (surveyFile == null || surveyFile.Length == 0)
                return BadRequest("Survey file is required");

            // Save files temporarily
            var tempSurveyPath = Path.Combine(Path.GetTempPath(), surveyFile.FileName);
            using (var stream = new FileStream(tempSurveyPath, FileMode.Create))
            {
                await surveyFile.CopyToAsync(stream);
            }

            string? tempDataPath = null;
            if (dataFile != null && dataFile.Length > 0)
            {
                tempDataPath = Path.Combine(Path.GetTempPath(), dataFile.FileName);
                using (var stream = new FileStream(tempDataPath, FileMode.Create))
                {
                    await dataFile.CopyToAsync(stream);
                }
            }

            // Use default project path if not provided
            var defaultProjectPath = Path.Combine(_environment.ContentRootPath, "SampleData", "Demo 2015.porp");
            var actualProjectPath = string.IsNullOrEmpty(projectPath) ? defaultProjectPath : projectPath;

            // Load the project
            var (survey, project, data) = ProjectLoader.LoadProject(
                tempSurveyPath,
                actualProjectPath,
                tempDataPath ?? tempSurveyPath
            );

            // Link data to survey
            survey.Data = data;

            // Save using persistence service if available, otherwise use basic repository
            Survey savedSurvey;
            if (_persistenceService != null)
            {
                savedSurvey = await _persistenceService.SaveSurveyWithDetailsAsync(survey, project);
            }
            else
            {
                savedSurvey = await _surveyRepository.AddAsync(survey);
            }

            // Clean up temp files
            System.IO.File.Delete(tempSurveyPath);
            if (tempDataPath != null)
                System.IO.File.Delete(tempDataPath);

            return Ok(new
            {
                Message = "Survey imported successfully",
                SurveyId = savedSurvey.Id,
                SurveyName = savedSurvey.SurveyName,
                QuestionCount = savedSurvey.QuestionList?.Count ?? 0,
                ResponseCount = data?.DataList?.Count - 1 ?? 0,
                ProjectName = project?.ProjectName
            });
        }
        catch (Exception ex)
        {
            return Problem($"Error importing survey: {ex.Message}");
        }
    }

    /// <summary>
    /// Import all surveys from a .porp project file
    /// Reads the project file and imports all referenced surveys
    /// </summary>
    [HttpPost("project")]
    public async Task<IActionResult> ImportProjectFile(
        [FromForm] IFormFile projectFile,
        [FromForm] IFormFile? surveyFile,
        [FromForm] IFormFile? dataFile)
    {
        try
        {
            if (projectFile == null || projectFile.Length == 0)
                return BadRequest("Project file (.porp) is required");


            // Save project file temporarily
            var tempProjectPath = Path.Combine(Path.GetTempPath(), projectFile.FileName);
            using (var stream = new FileStream(tempProjectPath, FileMode.Create))
            {
                await projectFile.CopyToAsync(stream);
            }

            // Save survey file if provided
            if (surveyFile != null && surveyFile.Length > 0)
            {
                var tempSurveyPath = Path.Combine(Path.GetTempPath(), surveyFile.FileName);
                using (var stream = new FileStream(tempSurveyPath, FileMode.Create))
                {
                    await surveyFile.CopyToAsync(stream);
                }
            }

            // Save data file if provided
            if (dataFile != null && dataFile.Length > 0)
            {
                var tempDataPath = Path.Combine(Path.GetTempPath(), dataFile.FileName);
                using (var stream = new FileStream(tempDataPath, FileMode.Create))
                {
                    await dataFile.CopyToAsync(stream);
                }
            }

            // List all *.porp* files in temp directory for troubleshooting
            var tempDir = Path.GetDirectoryName(tempProjectPath) ?? Path.GetTempPath();
            var porpFiles = Directory.GetFiles(tempDir, "*.porp*").Select(Path.GetFileName).ToList();
            Console.WriteLine("PORP-related files in temp directory:");
            foreach (var file in porpFiles)
            {
                Console.WriteLine($" - {file}");
            }

            // Import surveys using shared helper method
            var (importedSurveys, errors, referencedFiles) = await ImportSurveysFromDirectory(tempDir, tempProjectPath);

            // Load project metadata for response
            var project = ProjectLoader.LoadProjectOnly(tempProjectPath);

            // Clean up temp project file
            System.IO.File.Delete(tempProjectPath);

            return Ok(new
            {
                Message = $"Imported {importedSurveys.Count} of {project.SurveyListSummary?.Count ?? 0} surveys",
                ProjectName = project.ProjectName,
                ClientName = project.ClientName,
                TempPorpFiles = porpFiles,
                ReferencedFiles = referencedFiles,
                ImportedSurveys = importedSurveys,
                Errors = errors.Count > 0 ? errors : null
            });
        }
        catch (Exception ex)
        {
            return Problem($"Error importing project: {ex.Message}");
        }
    }

    /// <summary>
    /// Import a .porpz archive file (compressed survey package)
    /// </summary>
    [HttpPost("porpz")]
    public async Task<IActionResult> ImportPorpzArchive([FromForm] IFormFile porpzFile)
    {
        try
        {
            if (porpzFile == null || porpzFile.Length == 0)
                return BadRequest("PORPZ file is required");

            // Save the uploaded .porpz file to a temp location
            var tempZipPath = Path.Combine(Path.GetTempPath(), porpzFile.FileName);
            using (var stream = new FileStream(tempZipPath, FileMode.Create))
            {
                await porpzFile.CopyToAsync(stream);
            }

            // Create a temp directory for extraction
            var extractDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(tempZipPath) + "_extract");
            if (Directory.Exists(extractDir))
                Directory.Delete(extractDir, true);
            Directory.CreateDirectory(extractDir);

            // Extract the zip file
            ZipFile.ExtractToDirectory(tempZipPath, extractDir);

            // List extracted files
            var extractedFiles = Directory.GetFiles(extractDir, "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(extractDir, f)).ToList();

            // Find the .porp project file
            var projectFile = Directory.GetFiles(extractDir, "*.porp", SearchOption.AllDirectories).FirstOrDefault();
            if (projectFile == null)
            {
                Directory.Delete(extractDir, true);
                System.IO.File.Delete(tempZipPath);
                return BadRequest("No .porp project file found in archive");
            }

            // Use the same import logic as /project endpoint
            var (importedSurveys, errors, referencedFiles) = await ImportSurveysFromDirectory(extractDir, projectFile);

            // Load project metadata
            var project = ProjectLoader.LoadProjectOnly(projectFile);

            // Clean up
            System.IO.File.Delete(tempZipPath);
            Directory.Delete(extractDir, true);

            return Ok(new
            {
                Message = $"Imported {importedSurveys.Count} of {project.SurveyListSummary?.Count ?? 0} surveys from PORPZ archive",
                FileName = porpzFile.FileName,
                ProjectName = project.ProjectName,
                ClientName = project.ClientName,
                ExtractedFiles = extractedFiles,
                ReferencedFiles = referencedFiles,
                ImportedSurveys = importedSurveys,
                Errors = errors.Count > 0 ? errors : null
            });
        }
        catch (Exception ex)
        {
            return Problem($"Error importing PORPZ file: {ex.Message}");
        }
        finally
        {
            // Clean up extraction directory if it exists
            var extractDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(porpzFile.FileName) + "_extract");
            if (Directory.Exists(extractDir))
            {
                try { Directory.Delete(extractDir, true); } catch { }
            }
        }
    }

    /// <summary>
    /// Import from SPSS format
    /// </summary>
    [HttpPost("spss")]
    public async Task<IActionResult> ImportSpssFile([FromForm] IFormFile spssFile)
    {
        try
        {
            if (spssFile == null || spssFile.Length == 0)
                return BadRequest("SPSS file is required");

            // TODO: Implement SPSS import logic
            // Will require parsing .sav or .por files
            // and converting to Porpoise Survey model

            return StatusCode(501, new
            {
                Message = "SPSS import not yet implemented",
                FileName = spssFile.FileName,
                FileSize = spssFile.Length
            });
        }
        catch (Exception ex)
        {
            return Problem($"Error importing SPSS file: {ex.Message}");
        }
    }

    /// <summary>
    /// Import from CSV format
    /// </summary>
    [HttpPost("csv")]
    public async Task<IActionResult> ImportCsvFile(
        [FromForm] IFormFile csvFile,
        [FromForm] bool hasHeaders = true)
    {
        try
        {
            if (csvFile == null || csvFile.Length == 0)
                return BadRequest("CSV file is required");

            // TODO: Implement CSV import logic
            // Will require:
            // - Parsing CSV structure
            // - Inferring question types from data
            // - Creating Survey and SurveyData objects

            return StatusCode(501, new
            {
                Message = "CSV import not yet implemented",
                FileName = csvFile.FileName,
                FileSize = csvFile.Length,
                HasHeaders = hasHeaders
            });
        }
        catch (Exception ex)
        {
            return Problem($"Error importing CSV file: {ex.Message}");
        }
    }

    /// <summary>
    /// Get list of sample files available for import
    /// </summary>
    [HttpGet("samples")]
    public IActionResult GetSampleFiles()
    {
        try
        {
            var sampleDataPath = Path.Combine(_environment.ContentRootPath, "SampleData");
            
            if (!Directory.Exists(sampleDataPath))
                return Ok(new { Files = new List<object>() });

            var files = Directory.GetFiles(sampleDataPath, "*.*", SearchOption.TopDirectoryOnly)
                .Select(f => new
                {
                    FileName = Path.GetFileName(f),
                    Extension = Path.GetExtension(f),
                    SizeBytes = new FileInfo(f).Length,
                    LastModified = new FileInfo(f).LastWriteTime
                })
                .OrderBy(f => f.FileName)
                .ToList();

            return Ok(new { SampleDataPath = sampleDataPath, Files = files });
        }
        catch (Exception ex)
        {
            return Problem($"Error retrieving sample files: {ex.Message}");
        }
    }
}

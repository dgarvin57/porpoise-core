using Microsoft.AspNetCore.Mvc;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Data;
using Porpoise.Core.Models;

namespace Porpoise.Api.Controllers;

[ApiController]
[Route("api/survey-import")]
public class SurveyImportController : ControllerBase
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IWebHostEnvironment _environment;

    public SurveyImportController(
        ISurveyRepository surveyRepository,
        IWebHostEnvironment environment)
    {
        _surveyRepository = surveyRepository;
        _environment = environment;
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

            // Save to database (if not using in-memory mode)
            var savedSurvey = await _surveyRepository.AddAsync(survey);

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
    /// Import a .porpz archive file (compressed survey package)
    /// </summary>
    [HttpPost("porpz")]
    public async Task<IActionResult> ImportPorpzArchive([FromForm] IFormFile porpzFile)
    {
        try
        {
            if (porpzFile == null || porpzFile.Length == 0)
                return BadRequest("PORPZ file is required");

            // TODO: Implement .porpz extraction and import
            // .porpz files are typically compressed archives containing:
            // - .porps (survey definition)
            // - .porpd (response data)
            // - .porp (project metadata)
            // This will require zip extraction logic

            return StatusCode(501, new
            {
                Message = "PORPZ import not yet implemented",
                FileName = porpzFile.FileName,
                FileSize = porpzFile.Length
            });
        }
        catch (Exception ex)
        {
            return Problem($"Error importing PORPZ file: {ex.Message}");
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

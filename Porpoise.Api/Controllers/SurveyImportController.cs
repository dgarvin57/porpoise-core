using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Data;
using Porpoise.Core.DataAccess;
using Porpoise.Core.Models;
using Porpoise.Core.Services;

namespace Porpoise.Api.Controllers;

[ApiController]
[Route("api/survey-import")]
public class SurveyImportController : ControllerBase
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly SurveyPersistenceService? _persistenceService;
    private readonly IWebHostEnvironment _environment;

    public SurveyImportController(
        ISurveyRepository surveyRepository,
        IProjectRepository projectRepository,
        IWebHostEnvironment environment,
        SurveyPersistenceService? persistenceService = null)
    {
        _surveyRepository = surveyRepository;
        _projectRepository = projectRepository;
        _environment = environment;
        _persistenceService = persistenceService;
    }

    /// <summary>
    /// Ensures a project exists in the database, creating it with a unique name if necessary
    /// </summary>
    private async Task<Guid> EnsureProjectExistsAsync(Project project)
    {
        if (project == null || string.IsNullOrEmpty(project.ProjectName))
        {
            throw new ArgumentException("Project name is required");
        }

        // Always create a new project with a unique name (don't reuse existing)
        var originalName = project.ProjectName;
        project.ProjectName = await GetUniqueProjectNameAsync(project.ProjectName);
        
        // Generate a new ID (don't reuse the ID from the .porp file)
        project.Id = Guid.NewGuid();
        Console.WriteLine($"[PROJECT] Creating project: '{originalName}' -> '{project.ProjectName}'");
        
        var savedProject = await _projectRepository.AddAsync(project);
        Console.WriteLine($"[PROJECT] Saved with ID: {savedProject.Id}");
        return savedProject.Id;
    }

    /// <summary>
    /// Get a unique survey name by appending a number if name already exists
    /// </summary>
    private async Task<string> GetUniqueSurveyNameAsync(string baseName)
    {
        var existingSurveys = await _surveyRepository.GetAllAsync();
        var existingNames = existingSurveys.Select(s => s.SurveyName?.ToLower()).ToHashSet();
        
        if (!existingNames.Contains(baseName.ToLower()))
            return baseName;
        
        int counter = 2;
        string newName;
        do
        {
            newName = $"{baseName} ({counter})";
            counter++;
        } while (existingNames.Contains(newName.ToLower()));
        
        return newName;
    }

    /// <summary>
    /// Get a unique survey name within a specific project
    /// </summary>
    private async Task<string> GetUniqueSurveyNameInProjectAsync(string baseName, Guid projectId)
    {
        var allSurveys = await _surveyRepository.GetAllAsync();
        var projectSurveys = allSurveys.Where(s => s.ProjectId == projectId);
        var existingNames = projectSurveys.Select(s => s.SurveyName?.ToLower()).ToHashSet();
        
        if (!existingNames.Contains(baseName.ToLower()))
            return baseName;
        
        int counter = 2;
        string newName;
        do
        {
            newName = $"{baseName} ({counter})";
            counter++;
        } while (existingNames.Contains(newName.ToLower()));
        
        return newName;
    }

    /// <summary>
    /// Get a unique project name by appending a number if name already exists
    /// </summary>
    private async Task<string> GetUniqueProjectNameAsync(string baseName)
    {
        var existingProjects = await _projectRepository.GetAllAsync();
        var existingNames = existingProjects.Select(p => p.ProjectName?.ToLower()).ToHashSet();
        
        if (!existingNames.Contains(baseName.ToLower()))
            return baseName;
        
        int counter = 2;
        string newName;
        do
        {
            newName = $"{baseName} ({counter})";
            counter++;
        } while (existingNames.Contains(newName.ToLower()));
        
        return newName;
    }

    /// <summary>
    /// Helper method to import surveys from a directory containing .porp, .porps, and .porpd files
    /// </summary>
    private async Task<(List<object> ImportedSurveys, List<string> Errors, List<string> ReferencedFiles, Guid? ProjectId)> ImportSurveysFromDirectory(string directory, string? projectFilePath)
    {
        var importedSurveys = new List<object>();
        var errors = new List<string>();
        var referencedFiles = new List<string>();

        Project project;
        
        // If no project file, create a default project from survey files
        if (string.IsNullOrEmpty(projectFilePath))
        {
            var surveyFiles = Directory.GetFiles(directory, "*.porps", SearchOption.AllDirectories);
            if (surveyFiles.Length == 0)
            {
                errors.Add("No survey files found in archive");
                return (importedSurveys, errors, referencedFiles, null);
            }

            // Load first survey to get its name for the project
            var firstSurveyPath = surveyFiles[0];
            var (firstSurvey, _, _) = ProjectLoader.LoadProject(firstSurveyPath, null, firstSurveyPath);
            
            var surveySummaries = new ObjectListBase<SurveySummary>();
            foreach (var file in surveyFiles)
            {
                surveySummaries.Add(new SurveySummary
                {
                    SurveyFileName = Path.GetFileName(file)
                });
            }
            
            project = new Project
            {
                ProjectName = firstSurvey.SurveyName ?? "Imported Survey",
                Description = "Auto-created project from survey import",
                SurveyListSummary = surveySummaries
            };
            Console.WriteLine($"[IMPORT] Auto-created project from {surveyFiles.Length} survey file(s)");
        }
        else
        {
            // Load the project to get survey references
            project = ProjectLoader.LoadProjectOnly(projectFilePath);
            Console.WriteLine($"[IMPORT] Loaded project: '{project.ProjectName}' with {project.SurveyListSummary?.Count ?? 0} surveys");
        }
        
        if (project.SurveyListSummary == null || project.SurveyListSummary.Count == 0)
        {
            errors.Add("Project contains no survey references");
            return (importedSurveys, errors, referencedFiles, null);
        }

        // Ensure project exists in database
        Guid? projectId = null;
        try
        {
            Console.WriteLine($"[IMPORT] Calling EnsureProjectExistsAsync for '{project.ProjectName}'");
            projectId = await EnsureProjectExistsAsync(project);
            Console.WriteLine($"[IMPORT] Project created/found with ID: {projectId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IMPORT] Error in EnsureProjectExistsAsync: {ex.Message}");
            errors.Add($"Error creating project: {ex.Message}");
            return (importedSurveys, errors, referencedFiles, null);
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
                // Surveys are expected in the same folder as the project file
                string expectedSurveyPath = Path.Combine(projectFolder, surveySummary.SurveyFileName);
                
                string fallbackSurveyPath = Path.Combine(projectFolder, surveySummary.SurveyFileName);
                string surveyPath = System.IO.File.Exists(expectedSurveyPath) ? expectedSurveyPath : (System.IO.File.Exists(fallbackSurveyPath) ? fallbackSurveyPath : expectedSurveyPath);
                
                // Construct data path - should be in same directory as survey file, with .porpd extension
                var surveyDir = Path.GetDirectoryName(surveyPath) ?? projectFolder;
                var surveyBaseName = Path.GetFileNameWithoutExtension(surveyPath);
                
                // Try multiple patterns for the data file
                // Pattern 1: Same base name (e.g., "Cut Down 1.porps" -> "Cut Down 1.porpd")
                // Pattern 2: Remove spaces and add "dk" (e.g., "Cut Down 1.porps" -> "CutDown1dk.porpd" or "CutDowndk1.porpd")
                var surveyBaseNoSpaces = surveyBaseName.Replace(" ", "");
                var dataPaths = new[]
                {
                    Path.Combine(surveyDir, surveyBaseName + ".porpd"),           // Same base name
                    surveyPath.Replace(".porps", ".porpd"),                        // Simple extension replace
                    Path.Combine(surveyDir, surveyBaseNoSpaces + "dk.porpd"),     // No spaces + "dk"
                    Path.Combine(surveyDir, surveyBaseNoSpaces + "dk" + surveyBaseName.Last() + ".porpd")  // Pattern like "CutDowndk1.porpd"
                };
                
                // Also check for any .porpd file in the same directory
                string? dataPath = null;
                foreach (var path in dataPaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        dataPath = path;
                        break;
                    }
                }
                
                // If still not found, search for any .porpd file in the survey directory
                if (dataPath == null && Directory.Exists(surveyDir))
                {
                    var porpdFiles = Directory.GetFiles(surveyDir, "*.porpd");
                    if (porpdFiles.Length > 0)
                    {
                        dataPath = porpdFiles[0];
                    }
                }

                if (!System.IO.File.Exists(surveyPath))
                {
                    errors.Add($"Survey file not found: {surveySummary.SurveyFileName}\nTried: {expectedSurveyPath}\nAlso tried: {fallbackSurveyPath}");
                    continue;
                }

                // Load the survey - use data path if exists, otherwise pass surveyPath (will return empty data)
                var actualDataPath = dataPath ?? surveyPath;
                var (survey, _, data) = ProjectLoader.LoadProject(
                    surveyPath,
                    projectFilePath,
                    actualDataPath
                );

                // Make survey name unique within this project
                if (projectId.HasValue)
                {
                    survey.SurveyName = await GetUniqueSurveyNameInProjectAsync(survey.SurveyName, projectId.Value);
                }

                // IMPORTANT: Use the properly loaded data from .porpd, not metadata from .porps
                // Always prefer the data loaded from the .porpd file over what's in the .porps
                if (data != null && data.DataList != null && data.DataList.Count > 0)
                {
                    survey.Data = data;
                }
                else if (survey.Data != null && survey.Data.DataList != null && survey.Data.DataList.Count > 0)
                {
                    // Check if Data from .porps contains metadata instead of survey responses
                    var firstRow = survey.Data.DataList[0];
                    if (firstRow.Contains("CreatedOn") || firstRow.Contains("CreatedBy") || 
                        firstRow.Contains("ModifiedOn") || firstRow.Contains("IsDirty"))
                    {
                        // This is metadata, not survey data - clear it
                        survey.Data = new SurveyData { DataList = new List<List<string>>() };
                    }
                }
                
                survey.ProjectId = projectId; // Link survey to project

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

        return (importedSurveys, errors, referencedFiles, projectId);
    }

    /// <summary>
    /// Import a .porps/.porpd file pair
    /// </summary>
    [HttpPost("porps")]
    public async Task<IActionResult> ImportPorpsFile(
        [FromForm] IFormFile surveyFile,
        [FromForm] IFormFile? dataFile,
        [FromForm] IFormFile? projectFile,
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

            string? tempProjectPath = null;
            if (projectFile != null && projectFile.Length > 0)
            {
                tempProjectPath = Path.Combine(Path.GetTempPath(), projectFile.FileName);
                using (var stream = new FileStream(tempProjectPath, FileMode.Create))
                {
                    await projectFile.CopyToAsync(stream);
                }
            }

            // Load survey and data
            var survey = ProjectLoader.LoadSurvey(tempSurveyPath);
            var data = tempDataPath != null ? PorpoiseFileEncryption.ReadData(tempDataPath) : null;
            
            // Load project if uploaded
            Project? project = null;
            if (tempProjectPath != null)
            {
                Console.WriteLine($"[INDIVIDUAL] Loading project from file: {tempProjectPath}");
                project = ProjectLoader.LoadProjectOnly(tempProjectPath);
            }
            else
            {
                Console.WriteLine("[INDIVIDUAL] No project file uploaded");
            }

            // Ensure survey name is unique
            survey.SurveyName = await GetUniqueSurveyNameAsync(survey.SurveyName ?? "Imported Survey");

            // If no project file provided, create a default project
            if (project == null)
            {
                Console.WriteLine($"[INDIVIDUAL] Creating auto project for survey: {survey.SurveyName}");
                project = new Project
                {
                    ProjectName = survey.SurveyName,
                    Description = "Auto-created project from survey import"
                };
            }

            // Ensure project exists in database
            Guid? projectId = null;
            if (project != null)
            {
                Console.WriteLine($"[INDIVIDUAL] Calling EnsureProjectExistsAsync for: {project.ProjectName}");
                projectId = await EnsureProjectExistsAsync(project);
                Console.WriteLine($"[INDIVIDUAL] Project saved with ID: {projectId}");
                survey.ProjectId = projectId.Value;
            }

            // IMPORTANT: Use the properly loaded data from .porpd, not metadata from .porps
            if (data != null && data.DataList != null && data.DataList.Count > 0)
            {
                survey.Data = data;
            }
            else if (survey.Data != null && survey.Data.DataList != null && survey.Data.DataList.Count > 0)
            {
                // Check if Data contains metadata instead of survey responses
                var firstRow = survey.Data.DataList[0];
                if (firstRow.Contains("CreatedOn") || firstRow.Contains("CreatedBy") || 
                    firstRow.Contains("ModifiedOn") || firstRow.Contains("IsDirty"))
                {
                    // This is metadata, not survey data - clear it
                    survey.Data = new SurveyData { DataList = new List<List<string>>() };
                }
            }

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
            if (tempProjectPath != null)
                System.IO.File.Delete(tempProjectPath);

            return Ok(new
            {
                Message = "Survey imported successfully",
                SurveyId = savedSurvey.Id,
                SurveyName = savedSurvey.SurveyName,
                ProjectId = projectId,
                ProjectName = project?.ProjectName,
                QuestionCount = savedSurvey.QuestionList?.Count ?? 0,
                ResponseCount = data?.DataList?.Count - 1 ?? 0
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
            var (importedSurveys, errors, referencedFiles, projectId) = await ImportSurveysFromDirectory(tempDir, tempProjectPath);

            // Load project metadata for response
            var project = ProjectLoader.LoadProjectOnly(tempProjectPath);

            // Clean up temp project file
            System.IO.File.Delete(tempProjectPath);

            return Ok(new
            {
                Message = $"Imported {importedSurveys.Count} of {project.SurveyListSummary?.Count ?? 0} surveys",
                ProjectId = projectId,
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

            // Find the .porp project file (optional - will auto-create if missing)
            var projectFile = Directory.GetFiles(extractDir, "*.porp", SearchOption.AllDirectories).FirstOrDefault();

            // Use the same import logic as /project endpoint
            var (importedSurveys, errors, referencedFiles, projectId) = await ImportSurveysFromDirectory(extractDir, projectFile);

            // Load project metadata for response
            Project? project = null;
            if (projectFile != null)
            {
                project = ProjectLoader.LoadProjectOnly(projectFile);
            }
            else if (projectId.HasValue)
            {
                // Get the auto-created project from database
                project = await _projectRepository.GetByIdAsync(projectId.Value);
            }

            // Clean up
            System.IO.File.Delete(tempZipPath);
            Directory.Delete(extractDir, true);

            return Ok(new
            {
                Message = $"Imported {importedSurveys.Count} survey(s) from PORPZ archive",
                FileName = porpzFile.FileName,
                ProjectId = projectId,
                ProjectName = project?.ProjectName,
                ClientName = project?.ClientName,
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

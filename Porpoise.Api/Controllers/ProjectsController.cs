using Microsoft.AspNetCore.Mvc;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.Api.Models;

namespace Porpoise.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProjectsController(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
        {
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            var projects = await _projectRepository.GetAllAsync();
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            // Convert to response DTO with base64 logo
            var response = new ProjectResponse
            {
                Id = project.Id,
                TenantId = project.TenantId,
                ProjectName = project.ProjectName,
                ClientName = project.ClientName,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                DefaultWeightingScheme = project.DefaultWeightingScheme,
                BrandingSettings = project.BrandingSettings,
                ResearcherLabel = project.ResearcherLabel,
                ResearcherSubLabel = project.ResearcherSubLabel,
                ResearcherLogoBase64 = project.ResearcherLogo != null 
                    ? Convert.ToBase64String(project.ResearcherLogo) 
                    : null,
                IsDeleted = project.IsDeleted,
                DeletedDate = project.DeletedDate,
                // Date fields now match database column names
                CreatedDate = project.CreatedDate,
                ModifiedDate = project.ModifiedDate
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] Project project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _projectRepository.AddAsync(project);
            await _unitOfWork.CommitAsync();

            return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingProject = await _projectRepository.GetByIdAsync(id);
            if (existingProject == null)
            {
                return NotFound();
            }

            existingProject.ProjectName = request.ProjectName;
            existingProject.ClientName = request.ClientName;
            existingProject.Description = request.Description;
            existingProject.StartDate = request.StartDate;
            existingProject.EndDate = request.EndDate;
            existingProject.ResearcherLabel = request.ResearcherLabel;
            existingProject.ResearcherSubLabel = request.ResearcherSubLabel;
            existingProject.DefaultWeightingScheme = request.DefaultWeightingScheme;
            existingProject.BrandingSettings = request.BrandingSettings;

            // Convert base64 logo to byte array if provided
            if (!string.IsNullOrEmpty(request.ResearcherLogoBase64))
            {
                try
                {
                    // Remove data URL prefix if present (e.g., "data:image/png;base64,")
                    var base64Data = request.ResearcherLogoBase64;
                    if (base64Data.Contains(","))
                    {
                        base64Data = base64Data.Split(',')[1];
                    }
                    existingProject.ResearcherLogo = Convert.FromBase64String(base64Data);
                }
                catch (FormatException)
                {
                    return BadRequest("Invalid logo image format");
                }
            }

            await _projectRepository.UpdateAsync(existingProject);
            await _unitOfWork.CommitAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var existingProject = await _projectRepository.GetByIdAsync(id);
            if (existingProject == null)
            {
                return NotFound();
            }

            await _projectRepository.DeleteAsync(id);
            await _unitOfWork.CommitAsync();

            return NoContent();
        }

        [HttpGet("with-counts")]
        public async Task<IActionResult> GetProjectsWithCounts()
        {
            var projects = await _projectRepository.GetProjectsWithSurveyCountAsync();
            return Ok(projects);
        }

        [HttpGet("logos")]
        public async Task<IActionResult> GetProjectLogos()
        {
            var projects = await _projectRepository.GetAllAsync();
            var logos = projects
                .Where(p => p.ResearcherLogo != null)
                .Select(p => new
                {
                    id = p.Id.ToString(),
                    researcherLogoBase64 = Convert.ToBase64String(p.ResearcherLogo)
                });
            return Ok(logos);
        }

        [HttpGet("multi-survey")]
        public async Task<IActionResult> GetMultiSurveyProjects()
        {
            var projects = await _projectRepository.GetMultiSurveyProjectsAsync();
            return Ok(projects);
        }

        [HttpGet("{id}/surveys")]
        public async Task<IActionResult> GetProjectSurveys(Guid id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var surveys = await _projectRepository.GetSurveysWithCountsAsync(id);
            return Ok(surveys);
        }

        [HttpPost("{projectId:guid}/surveys/{surveyId:guid}/move")]
        public async Task<IActionResult> MoveSurvey(Guid projectId, Guid surveyId)
        {
            var success = await _projectRepository.MoveSurveyToProjectAsync(surveyId, projectId);
            if (!success)
            {
                return NotFound();
            }

            await _unitOfWork.CommitAsync();
            return Ok(new { message = "Survey moved successfully" });
        }

        [HttpPost("{id:guid}/soft-delete")]
        public async Task<IActionResult> SoftDeleteProject(Guid id)
        {
            var success = await _projectRepository.SoftDeleteProjectAsync(id);
            if (!success)
            {
                return NotFound();
            }

            await _unitOfWork.CommitAsync();
            return Ok(new { message = "Project moved to trash" });
        }

        [HttpPost("{id:guid}/restore")]
        public async Task<IActionResult> RestoreProject(Guid id)
        {
            var success = await _projectRepository.RestoreProjectAsync(id);
            if (!success)
            {
                return NotFound();
            }

            await _unitOfWork.CommitAsync();
            return Ok(new { message = "Project restored" });
        }

        [HttpDelete("{id:guid}/permanent")]
        public async Task<IActionResult> PermanentlyDeleteProject(Guid id)
        {
            try
            {
                var success = await _projectRepository.PermanentlyDeleteProjectAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                await _unitOfWork.CommitAsync();
                return Ok(new { message = "Project permanently deleted" });
            }
            catch (Exception ex)
            {
                return Problem($"Error permanently deleting project: {ex.Message}");
            }
        }

        [HttpGet("trash")]
        public async Task<IActionResult> GetDeletedProjects()
        {
            var projects = await _projectRepository.GetDeletedProjectsAsync();
            return Ok(projects);
        }
    }
}
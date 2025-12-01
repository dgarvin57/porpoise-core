using Microsoft.AspNetCore.Mvc;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;

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
            return Ok(project);
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
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] Project project)
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

            existingProject.ProjectName = project.ProjectName;
            existingProject.ClientName = project.ClientName;
            existingProject.Description = project.Description;
            existingProject.StartDate = project.StartDate;
            existingProject.EndDate = project.EndDate;
            existingProject.DefaultWeightingScheme = project.DefaultWeightingScheme;
            existingProject.BrandingSettings = project.BrandingSettings;

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

        [HttpPost("{projectId}/surveys/{surveyId}/move")]
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
    }
}
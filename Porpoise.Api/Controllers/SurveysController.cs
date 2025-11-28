using Microsoft.AspNetCore.Mvc;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;

namespace Porpoise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SurveysController : ControllerBase
{
    private readonly ISurveyRepository _surveyRepository;

    public SurveysController(ISurveyRepository surveyRepository)
    {
        _surveyRepository = surveyRepository;
    }

    /// <summary>
    /// Get all surveys
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllSurveys()
    {
        try
        {
            var surveys = await _surveyRepository.GetAllAsync();
            return Ok(surveys);
        }
        catch (Exception ex)
        {
            return Problem($"Error retrieving surveys: {ex.Message}");
        }
    }

    /// <summary>
    /// Get survey by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSurveyById(Guid id)
    {
        try
        {
            var survey = await _surveyRepository.GetByIdAsync(id);
            return survey is null 
                ? NotFound($"Survey with ID {id} not found") 
                : Ok(survey);
        }
        catch (Exception ex)
        {
            return Problem($"Error retrieving survey: {ex.Message}");
        }
    }

    /// <summary>
    /// Get survey by name
    /// </summary>
    [HttpGet("name/{name}")]
    public async Task<IActionResult> GetSurveyByName(string name)
    {
        try
        {
            var survey = await _surveyRepository.GetByNameAsync(name);
            return survey is null 
                ? NotFound($"Survey '{name}' not found") 
                : Ok(survey);
        }
        catch (Exception ex)
        {
            return Problem($"Error retrieving survey: {ex.Message}");
        }
    }

    /// <summary>
    /// Search surveys by name
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchSurveys([FromQuery] string term)
    {
        try
        {
            var surveys = await _surveyRepository.SearchByNameAsync(term);
            return Ok(surveys);
        }
        catch (Exception ex)
        {
            return Problem($"Error searching surveys: {ex.Message}");
        }
    }

    /// <summary>
    /// Get surveys by status
    /// </summary>
    [HttpGet("status/{status:int}")]
    public async Task<IActionResult> GetSurveysByStatus(int status)
    {
        try
        {
            var surveys = await _surveyRepository.GetByStatusAsync((SurveyStatus)status);
            return Ok(surveys);
        }
        catch (Exception ex)
        {
            return Problem($"Error retrieving surveys by status: {ex.Message}");
        }
    }

    /// <summary>
    /// Create new survey
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateSurvey([FromBody] Survey survey)
    {
        try
        {
            // Validate survey name
            survey.ValidateSurveyName();
            
            // Check if name already exists
            if (await _surveyRepository.SurveyNameExistsAsync(survey.SurveyName))
            {
                return Conflict($"Survey with name '{survey.SurveyName}' already exists");
            }

            var created = await _surveyRepository.AddAsync(survey);
            return CreatedAtAction(
                nameof(GetSurveyById), 
                new { id = created.Id }, 
                created
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Problem($"Error creating survey: {ex.Message}");
        }
    }

    /// <summary>
    /// Update existing survey
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSurvey(Guid id, [FromBody] Survey survey)
    {
        try
        {
            // Ensure ID matches
            if (id != survey.Id)
            {
                return BadRequest("Survey ID mismatch");
            }

            // Check if survey exists
            if (!await _surveyRepository.ExistsAsync(id))
            {
                return NotFound($"Survey with ID {id} not found");
            }

            // Validate survey name
            survey.ValidateSurveyName();

            // Check if name already exists (excluding current survey)
            if (await _surveyRepository.SurveyNameExistsAsync(survey.SurveyName, id))
            {
                return Conflict($"Another survey with name '{survey.SurveyName}' already exists");
            }

            var updated = await _surveyRepository.UpdateAsync(survey);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return Problem($"Error updating survey: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete survey
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSurvey(Guid id)
    {
        try
        {
            if (!await _surveyRepository.ExistsAsync(id))
            {
                return NotFound($"Survey with ID {id} not found");
            }

            var deleted = await _surveyRepository.DeleteAsync(id);
            return deleted 
                ? NoContent() 
                : Problem("Failed to delete survey");
        }
        catch (Exception ex)
        {
            return Problem($"Error deleting survey: {ex.Message}");
        }
    }

    /// <summary>
    /// Get survey statistics
    /// </summary>
    [HttpGet("{id:guid}/stats")]
    public async Task<IActionResult> GetSurveyStats(Guid id)
    {
        try
        {
            if (!await _surveyRepository.ExistsAsync(id))
            {
                return NotFound($"Survey with ID {id} not found");
            }

            var questionCount = await _surveyRepository.GetQuestionCountAsync(id);
            var responseCount = await _surveyRepository.GetResponseCountAsync(id);

            return Ok(new
            {
                SurveyId = id,
                QuestionCount = questionCount,
                ResponseCount = responseCount
            });
        }
        catch (Exception ex)
        {
            return Problem($"Error retrieving survey stats: {ex.Message}");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.Core.Services;

namespace Porpoise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SurveysController : ControllerBase
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IResponseRepository _responseRepository;
    private readonly ISurveyDataRepository _surveyDataRepository;

    public SurveysController(
        ISurveyRepository surveyRepository,
        IQuestionRepository questionRepository,
        IResponseRepository responseRepository,
        ISurveyDataRepository surveyDataRepository)
    {
        _surveyRepository = surveyRepository;
        _questionRepository = questionRepository;
        _responseRepository = responseRepository;
        _surveyDataRepository = surveyDataRepository;
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
            var survey = await _surveyRepository.GetByIdAsync(id);
            if (survey == null)
            {
                return NotFound($"Survey with ID {id} not found");
            }

            // Get questions from database
            var questions = await _questionRepository.GetBySurveyIdAsync(id);
            
            // Get total case count from survey data JSON blob
            int totalCases = await _surveyDataRepository.GetCaseCountBySurveyIdAsync(id);

            return Ok(new
            {
                questionCount = questions.Count(),
                responseCount = totalCases
            });
        }
        catch (Exception ex)
        {
            return Problem($"Error retrieving survey stats: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all questions with response statistics for a survey
    /// </summary>
    [HttpGet("{id:guid}/questions")]
    public async Task<IActionResult> GetSurveyQuestions(Guid id)
    {
        try
        {
            var survey = await _surveyRepository.GetByIdAsync(id);
            if (survey == null)
            {
                return NotFound($"Survey with ID {id} not found");
            }

            // Get questions from database
            var questions = await _questionRepository.GetBySurveyIdAsync(id);
            
            // Get survey data (actual responses from survey takers)
            var surveyData = await _surveyDataRepository.GetBySurveyIdAsync(id);
            
            // Load responses for each question
            var questionResults = new List<object>();
            foreach (var question in questions)
            {
                var responseDefinitions = await _responseRepository.GetByQuestionIdAsync(question.Id);
                
                // Get missing value range for this question (MissingLow to MissingHigh, e.g., 97-99)
                var missingLow = (int)Math.Round(question.MissingLow);
                var missingHigh = (int)Math.Round(question.MissingHigh);
                
                // Calculate actual frequencies from survey data
                var responsesWithStats = new List<object>();
                int totalValidCases = 0; // Count of non-missing responses
                
                if (surveyData?.DataList != null && surveyData.DataList.Count > 1) // Need at least header + 1 data row
                {
                    // First, count total valid cases (excluding missing values)
                    // Skip row 0 which is the header row
                    for (int i = 1; i < surveyData.DataList.Count; i++)
                    {
                        var row = surveyData.DataList[i];
                        if (question.DataFileCol < row.Count)
                        {
                            if (int.TryParse(row[question.DataFileCol], out int value))
                            {
                                // Exclude values in the missing range
                                if (missingLow > 0 && value >= missingLow && value <= missingHigh)
                                {
                                    continue; // Skip missing values
                                }
                                totalValidCases++;
                            }
                        }
                    }
                    
                    // Now calculate frequencies for each response
                    foreach (var response in responseDefinitions)
                    {
                        int count = 0;
                        // Skip row 0 which is the header row
                        for (int i = 1; i < surveyData.DataList.Count; i++)
                        {
                            var row = surveyData.DataList[i];
                            if (question.DataFileCol < row.Count && 
                                row[question.DataFileCol] == response.RespValue.ToString())
                            {
                                count++;
                            }
                        }
                        
                        double percentage = totalValidCases > 0 ? (count / (double)totalValidCases * 100) : 0;
                        
                        // Get index symbol from IndexType: 0=None, 1=Neutral(/), 2=Positive(+), 3=Negative(-)
                        string indexSymbol = response.IndexType switch
                        {
                            ResponseIndexType.Positive => "+",
                            ResponseIndexType.Negative => "-",
                            ResponseIndexType.Neutral => "/",
                            _ => ""
                        };
                        
                        // Calculate index value (100 for base response, scaled for others)
                        string indexValue = response.RespValue == 1 ? "100.0" : 
                                          (percentage > 0 ? (percentage / 0.304 * 100).ToString("F1") : "—");
                        
                        responsesWithStats.Add(new
                        {
                            Label = response.Label,
                            Count = count,
                            Percentage = percentage,
                            Index = indexValue,
                            IndexSymbol = indexSymbol
                        });
                    }
                }
                else
                {
                    // No data available, just return response definitions with zero counts
                    foreach (var response in responseDefinitions)
                    {
                        string indexSymbol = response.IndexType switch
                        {
                            ResponseIndexType.Positive => "+",
                            ResponseIndexType.Negative => "-",
                            ResponseIndexType.Neutral => "/",
                            _ => ""
                        };
                        
                        responsesWithStats.Add(new
                        {
                            Label = response.Label,
                            Count = 0,
                            Percentage = 0.0,
                            Index = "—",
                            IndexSymbol = indexSymbol
                        });
                    }
                }
                
                questionResults.Add(new
                {
                    Id = question.Id.ToString(),
                    Label = question.QstLabel,
                    Text = question.QstStem,
                    Index = 128, // Default index value
                    TotalCases = totalValidCases, // Only count valid responses (exclude missing values)
                    Responses = responsesWithStats
                });
            }

            return Ok(questionResults);
        }
        catch (Exception ex)
        {
            return Problem($"Error retrieving survey questions: {ex.Message}");
        }
    }



}

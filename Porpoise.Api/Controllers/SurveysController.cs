using Microsoft.AspNetCore.Mvc;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;
using Porpoise.Core.Services;
using Porpoise.Core.Engines;

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
    /// Partially update survey (e.g., just survey notes, name, or status)
    /// </summary>
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> PatchSurvey(Guid id, [FromBody] Dictionary<string, object> updates)
    {
        try
        {
            var survey = await _surveyRepository.GetByIdAsync(id);
            if (survey == null)
            {
                return NotFound($"Survey with ID {id} not found");
            }

            // Apply updates
            if (updates.ContainsKey("surveyNotes"))
            {
                survey.SurveyNotes = updates["surveyNotes"]?.ToString() ?? string.Empty;
            }
            
            if (updates.ContainsKey("surveyName"))
            {
                var newName = updates["surveyName"]?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    // Check if name already exists (excluding current survey)
                    if (await _surveyRepository.SurveyNameExistsAsync(newName, id))
                    {
                        return Conflict($"Another survey with name '{newName}' already exists");
                    }
                    survey.SurveyName = newName;
                }
            }
            
            if (updates.ContainsKey("status"))
            {
                if (updates["status"] is int statusValue)
                {
                    survey.Status = (SurveyStatus)statusValue;
                }
                else if (int.TryParse(updates["status"]?.ToString(), out int parsedStatus))
                {
                    survey.Status = (SurveyStatus)parsedStatus;
                }
            }

            var updated = await _surveyRepository.UpdateAsync(survey);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return Problem($"Error updating survey: {ex.Message}");
        }
    }

    /// <summary>
    /// Partially update question (e.g., just question notes)
    /// </summary>
    [HttpPatch("{surveyId:guid}/questions/{questionId:guid}")]
    public async Task<IActionResult> PatchQuestion(Guid surveyId, Guid questionId, [FromBody] Dictionary<string, object> updates)
    {
        try
        {
            var question = await _questionRepository.GetByIdAsync(questionId);
            if (question == null)
            {
                return NotFound($"Question with ID {questionId} not found");
            }

            // Use targeted update for partial fields to avoid overwriting other data
            bool updated = false;
            if (updates.ContainsKey("questionNotes"))
            {
                updated = await _questionRepository.UpdateQuestionNotesAsync(questionId, updates["questionNotes"]?.ToString() ?? string.Empty);
            }

            if (!updated)
            {
                return Problem("Failed to update question");
            }

            // Return the updated question with full data (including joins)
            var updatedQuestion = await _questionRepository.GetByIdAsync(questionId);
            return Ok(updatedQuestion);
        }
        catch (Exception ex)
        {
            return Problem($"Error updating question: {ex.Message}");
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
    /// Get raw question list for a survey (for question editor/tree view)
    /// </summary>
    [HttpGet("{id:guid}/questions-list")]
    public async Task<IActionResult> GetSurveyQuestionsList(Guid id)
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
            
            // Return questions with camelCase field names for frontend
            var questionList = questions.Select(q => new
            {
                id = q.Id,
                qstNumber = q.QstNumber,
                qstLabel = q.QstLabel,
                qstStem = q.QstStem,
                dataFileCol = q.DataFileCol,
                variableType = (int)q.VariableType,
                blkQstStatus = (int?)q.BlkQstStatus,
                blkLabel = q.BlkLabel,
                blkStem = q.BlkStem,
                missValue1 = q.MissValue1,
                missValue2 = q.MissValue2,
                missValue3 = q.MissValue3,
                questionNotes = q.QuestionNotes
            });

            return Ok(questionList);
        }
        catch (Exception ex)
        {
            return Problem($"Error retrieving question list: {ex.Message}");
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

            // Get questions from database and sort by DataFileCol (numeric column) instead of QstNumber (text)
            var questions = (await _questionRepository.GetBySurveyIdAsync(id))
                .OrderBy(q => q.DataFileCol)
                .ToList();
            
            // Get survey data (actual responses from survey takers)
            var surveyData = await _surveyDataRepository.GetBySurveyIdAsync(id);
            
            // Assign survey data to survey object so calculations work
            survey.Data = surveyData;
            
            // Load responses for each question
            var questionResults = new List<object>();
            foreach (var question in questions)
            {
                var responseDefinitions = await _responseRepository.GetByQuestionIdAsync(question.Id);
                var responseList = new ObjectListBase<Response>();
                foreach (var resp in responseDefinitions)
                {
                    responseList.Add(resp);
                }
                question.Responses = responseList; // Assign responses to question
                
                // Get missing values for this question (discrete values like 97, 98, 99)
                var missingValues = question.MissingValues; // Returns List<int> from MissValue1/2/3
                
                // Calculate actual frequencies from survey data
                var responsesWithStats = new List<object>();
                int totalValidCases = 0; // Count of non-missing responses (weighted)
                int totalValidCasesUnweighted = 0; // Count of non-missing responses (unweighted)
                
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
                                // Exclude missing values
                                if (missingValues.Contains(value))
                                {
                                    continue; // Skip missing values
                                }
                                
                                // Find the response definition to get its weight
                                var responseDef = responseDefinitions.FirstOrDefault(r => r.RespValue == value);
                                double weight = responseDef?.Weight ?? 1.0;
                                
                                totalValidCases += (int)Math.Round(weight); // Weighted count
                                totalValidCasesUnweighted++; // Unweighted count
                            }
                        }
                    }
                    
                    // Set question TotalN for calculations
                    question.TotalN = totalValidCases;
                    
                    // Now calculate frequencies for each response and set ResultFrequency
                    foreach (var response in responseDefinitions)
                    {
                        int count = 0; // Weighted count
                        int countUnweighted = 0; // Unweighted count
                        double weight = response.Weight;
                        
                        // Skip row 0 which is the header row
                        for (int i = 1; i < surveyData.DataList.Count; i++)
                        {
                            var row = surveyData.DataList[i];
                            if (question.DataFileCol < row.Count && 
                                row[question.DataFileCol] == response.RespValue.ToString())
                            {
                                count += (int)Math.Round(weight); // Apply weight
                                countUnweighted++; // No weight applied
                            }
                        }
                        
                        // Set ResultFrequency for the calculation engine
                        response.ResultFrequency = count;
                        
                        double percentage = totalValidCases > 0 ? (count / (double)totalValidCases * 100) : 0;
                        
                        // Calculate index value for this response (100 = average, >100 = above average, <100 = below average)
                        // Index is calculated as (percentage / expected percentage) * 100
                        // For now, we'll use a simple approach based on IndexType
                        int indexValue = 100; // Default neutral
                        if (response.IndexType == ResponseIndexType.Positive && percentage > 0)
                        {
                            indexValue = (int)Math.Round(100 + (percentage * 0.5)); // Above average
                        }
                        else if (response.IndexType == ResponseIndexType.Negative && percentage > 0)
                        {
                            indexValue = (int)Math.Round(100 - (percentage * 0.5)); // Below average
                        }
                        
                        // Get index symbol from IndexType: 0=None, 1=Neutral(/), 2=Positive(+), 3=Negative(-)
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
                            Count = count,
                            CountUnweighted = countUnweighted,
                            Percentage = percentage,
                            Index = indexValue,
                            IndexSymbol = indexSymbol,
                            Weight = weight
                        });
                    }
                    
                    // Now calculate TotalIndex and other statistics using the QuestionEngine
                    QuestionEngine.CalculateStatisticsHelper(question, totalValidCasesUnweighted);
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
                            CountUnweighted = 0,
                            Percentage = 0.0,
                            Index = 100, // Neutral index when no data
                            IndexSymbol = indexSymbol,
                            Weight = response.Weight
                        });
                    }
                }
                
                questionResults.Add(new
                {
                    Id = question.Id.ToString(),
                    QstNumber = question.QstNumber,
                    Label = question.QstLabel,
                    Text = question.QstStem,
                    Index = question.TotalIndex, // Calculate from positive/negative responses
                    SamplingError = (double)question.SamplingError, // Question-level CI
                    TotalCases = totalValidCases, // Weighted count (exclude missing values)
                    TotalCasesUnweighted = totalValidCasesUnweighted, // Unweighted count (exclude missing values)
                    VariableType = (int)question.VariableType,
                    BlkQstStatus = (int?)question.BlkQstStatus,
                    BlkLabel = question.BlkLabel,
                    BlkStem = question.BlkStem,
                    BlockStem = question.Block?.Stem ?? question.BlkStem, // Use Block.Stem if available, fallback to deprecated BlkStem
                    QuestionNotes = question.QuestionNotes,
                    DataFileCol = question.DataFileCol,
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

    /// <summary>
    /// Soft delete a survey
    /// </summary>
    [HttpPost("{id:guid}/soft-delete")]
    public async Task<IActionResult> SoftDeleteSurvey(Guid id)
    {
        try
        {
            var success = await _surveyRepository.SoftDeleteSurveyAsync(id);
            if (!success)
            {
                return NotFound($"Survey with ID {id} not found");
            }

            return Ok(new { message = "Survey moved to trash" });
        }
        catch (Exception ex)
        {
            return Problem($"Error deleting survey: {ex.Message}");
        }
    }

    /// <summary>
    /// Restore a soft-deleted survey
    /// </summary>
    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> RestoreSurvey(Guid id)
    {
        try
        {
            var success = await _surveyRepository.RestoreSurveyAsync(id);
            if (!success)
            {
                return NotFound($"Survey with ID {id} not found");
            }

            return Ok(new { message = "Survey restored" });
        }
        catch (Exception ex)
        {
            return Problem($"Error restoring survey: {ex.Message}");
        }
    }

    /// <summary>
    /// Permanently delete a survey
    /// </summary>
    [HttpDelete("{id:guid}/permanent")]
    public async Task<IActionResult> PermanentlyDeleteSurvey(Guid id)
    {
        try
        {
            var success = await _surveyRepository.PermanentlyDeleteSurveyAsync(id);
            if (!success)
            {
                return NotFound($"Survey with ID {id} not found");
            }

            return Ok(new { message = "Survey permanently deleted" });
        }
        catch (Exception ex)
        {
            return Problem($"Error permanently deleting survey: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all deleted surveys (trash)
    /// </summary>
    [HttpGet("trash")]
    public async Task<IActionResult> GetDeletedSurveys()
    {
        try
        {
            var surveys = await _surveyRepository.GetDeletedSurveysAsync();
            return Ok(surveys);
        }
        catch (Exception ex)
        {
            return Problem($"Error retrieving deleted surveys: {ex.Message}");
        }
    }

    /// <summary>
    /// Get raw survey data (DataList) for a survey
    /// </summary>
    [HttpGet("{id:guid}/data")]
    public async Task<IActionResult> GetSurveyData(Guid id)
    {
        try
        {
            var surveyData = await _surveyDataRepository.GetBySurveyIdAsync(id);
            
            if (surveyData == null || surveyData.DataList == null || surveyData.DataList.Count == 0)
            {
                return NotFound($"No data found for survey {id}");
            }

            return Ok(new
            {
                TotalRows = surveyData.DataList.Count,
                HeaderRow = surveyData.DataList[0],
                DataRows = surveyData.DataList.Count - 1,
                DataList = surveyData.DataList,
                DataFilePath = surveyData.DataFilePath
            });
        }
        catch (Exception ex)
        {
            return Problem($"Error retrieving survey data: {ex.Message}");
        }
    }

    /// <summary>
    /// Get results for a specific question in a survey
    /// </summary>
    [HttpGet("{id:guid}/questions/{questionId:guid}/results")]
    public async Task<IActionResult> GetQuestionResults(Guid id, Guid questionId)
    {
        try
        {
            var survey = await _surveyRepository.GetByIdAsync(id);
            if (survey == null)
            {
                return NotFound($"Survey with ID {id} not found");
            }

            var question = await _questionRepository.GetByIdAsync(questionId);
            if (question == null)
            {
                return NotFound($"Question with ID {questionId} not found");
            }

            // Load responses for the question
            var responses = await _responseRepository.GetByQuestionIdAsync(questionId);
            question.Responses = new ObjectListBase<Response>(responses);

            // Load survey data for calculations
            var surveyData = await _surveyDataRepository.GetBySurveyIdAsync(id);
            survey.Data = surveyData;

            // Calculate statistics for this question
            QuestionEngine.CalculateQuestionAndResponseStatistics(survey, question);

            // Return question data with calculated results
            var results = question.Responses
                .Where(r => !question.MissingValues.Contains(r.RespValue))
                .Select(r => new
                {
                    r.RespValue,
                    RespLabel = r.Label,
                    Count = r.ResultFrequency,
                    Percent = Math.Round((decimal)r.ResultPercent * 100, 1)
                })
                .ToList();

            return Ok(new
            {
                question.Id,
                Label = question.QstLabel,
                question.QstNumber,
                question.VariableType,
                question.TotalN,
                Results = results
            });
        }
        catch (Exception ex)
        {
            return Problem($"Error retrieving question results: {ex.Message}");
        }
    }

}


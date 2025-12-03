#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Porpoise.Core.Application.Interfaces;
using Porpoise.Core.Models;

namespace Porpoise.Core.Services;

/// <summary>
/// Service for persisting complete surveys with questions and responses to the database.
/// Handles the full object graph: Survey → Questions → Responses → QuestionBlocks
/// </summary>
public class SurveyPersistenceService
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IResponseRepository _responseRepository;
    private readonly ISurveyDataRepository _surveyDataRepository;
    private readonly IProjectRepository? _projectRepository;
    private readonly IQuestionBlockRepository _questionBlockRepository;

    public SurveyPersistenceService(
        ISurveyRepository surveyRepository,
        IQuestionRepository questionRepository,
        IResponseRepository responseRepository,
        ISurveyDataRepository surveyDataRepository,
        IQuestionBlockRepository questionBlockRepository,
        IProjectRepository? projectRepository = null)
    {
        _surveyRepository = surveyRepository;
        _questionRepository = questionRepository;
        _responseRepository = responseRepository;
        _surveyDataRepository = surveyDataRepository;
        _questionBlockRepository = questionBlockRepository;
        _projectRepository = projectRepository;
    }

    /// <summary>
    /// Save a complete survey with all questions and responses to the database
    /// </summary>
    public async Task<Survey> SaveSurveyWithDetailsAsync(Survey survey, Project? project = null)
    {
        // Step 1: Save or find project if provided
        if (project != null && _projectRepository != null && !string.IsNullOrEmpty(project.ProjectName))
        {
            var existingProject = await _projectRepository.GetByNameAsync(project.ProjectName);
            if (existingProject != null)
            {
                survey.ProjectId = existingProject.Id;
            }
            else
            {
                var savedProject = await _projectRepository.AddAsync(project);
                survey.ProjectId = savedProject.Id;
            }
        }

        // Step 2: Save the survey
        var savedSurvey = await _surveyRepository.AddAsync(survey);

        // Step 3: Create QuestionBlocks for unique blocks in this survey
        var blockMap = new Dictionary<string, Guid>(); // blkLabel -> BlockId
        if (survey.QuestionList != null && survey.QuestionList.Count > 0)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            // Find all unique blocks (first question in each block has the block info)
            var firstQuestionsInBlocks = survey.QuestionList
                .Where(q => q.BlkQstStatus == BlkQuestionStatusType.FirstQuestionInBlock &&
                           !string.IsNullOrEmpty(q.BlkLabel))
                .ToList();

            int displayOrder = 0;
            foreach (var firstQuestion in firstQuestionsInBlocks)
            {
                // Create QuestionBlock record
                var questionBlock = new QuestionBlock
                {
                    SurveyId = savedSurvey.Id,
                    Label = firstQuestion.BlkLabel,
                    Stem = firstQuestion.BlkStem ?? string.Empty,
                    DisplayOrder = displayOrder++
                };

                var savedBlock = await _questionBlockRepository.AddAsync(questionBlock);
                blockMap[firstQuestion.BlkLabel] = savedBlock.Id;
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        // Step 4: Save questions and their responses
        if (survey.QuestionList != null && survey.QuestionList.Count > 0)
        {
            Guid? currentBlockId = null;
#pragma warning disable CS0618 // Type or member is obsolete
            
            foreach (var question in survey.QuestionList)
            {
                // Assign BlockId based on question's block status
                if (question.BlkQstStatus == BlkQuestionStatusType.FirstQuestionInBlock)
                {
                    // First question in block - use its label to find the BlockId
                    if (!string.IsNullOrEmpty(question.BlkLabel) && blockMap.ContainsKey(question.BlkLabel))
                    {
                        currentBlockId = blockMap[question.BlkLabel];
                        question.BlockId = currentBlockId;
                    }
                }
                else if (question.BlkQstStatus == BlkQuestionStatusType.ContinuationQuestion)
                {
                    // Continuation question - use current block's ID
                    question.BlockId = currentBlockId;
                }
                // Discrete questions (status 0 or 3) have BlockId = null
                
#pragma warning restore CS0618 // Type or member is obsolete
                var savedQuestion = await _questionRepository.AddAsync(question, savedSurvey.Id);

                // Save responses for this question
                if (question.Responses != null && question.Responses.Count > 0)
                {
                    foreach (var response in question.Responses)
                    {
                        // Note: ResultFrequency and ResultPercent are calculated fields, not persisted
                        await _responseRepository.AddAsync(response, savedQuestion.Id);
                    }
                }
            }
        }

        // Step 5: Save survey data (respondent answers)
        if (survey.Data != null && survey.Data.DataList != null && survey.Data.DataList.Count > 0)
        {
            await _surveyDataRepository.AddAsync(survey.Data, savedSurvey.Id);
        }

        return savedSurvey;
    }

    /// <summary>
    /// Save multiple surveys (e.g., from a project import)
    /// </summary>
    public async Task<List<Survey>> SaveMultipleSurveysAsync(List<Survey> surveys, Project? project = null)
    {
        var savedSurveys = new List<Survey>();

        foreach (var survey in surveys)
        {
            try
            {
                var saved = await SaveSurveyWithDetailsAsync(survey, project);
                savedSurveys.Add(saved);
            }
            catch (Exception ex)
            {
                // Log error but continue with other surveys
                Console.WriteLine($"Error saving survey '{survey.SurveyName}': {ex.Message}");
            }
        }

        return savedSurveys;
    }

    /// <summary>
    /// Check if a survey with the same name already exists
    /// </summary>
    public async Task<bool> SurveyExistsAsync(string surveyName)
    {
        return await _surveyRepository.SurveyNameExistsAsync(surveyName);
    }

    /// <summary>
    /// Get survey statistics after import
    /// </summary>
    public async Task<SurveyImportStats> GetImportStatsAsync(Guid surveyId)
    {
        var survey = await _surveyRepository.GetByIdAsync(surveyId);
        if (survey == null)
            throw new ArgumentException($"Survey {surveyId} not found");

        var questionCount = await _surveyRepository.GetQuestionCountAsync(surveyId);
        var responseCount = await _surveyRepository.GetResponseCountAsync(surveyId);

        return new SurveyImportStats
        {
            SurveyId = surveyId,
            SurveyName = survey.SurveyName,
            QuestionCount = questionCount,
            ResponseCount = responseCount,
            ImportedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Statistics about an imported survey
/// </summary>
public class SurveyImportStats
{
    public Guid SurveyId { get; set; }
    public string SurveyName { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public int ResponseCount { get; set; }
    public DateTime ImportedAt { get; set; }
}

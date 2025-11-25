#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Porpoise.Core.Models;

namespace Porpoise.Core.Engines;

/// <summary>
/// Builds a "Profile" report — shows % of respondents selecting a specific DV response
/// across all Independent Variables (IVs). Used in the Profile tab.
/// </summary>
public class ProfileEngine
{
    private readonly Survey _survey;
    private readonly Question _dvQuestion;

    public List<ProfileItem> Percentages { get; } = [];

    public ProfileEngine(Survey survey, Question dvQuestion, Response response)
    {
        _survey = survey ?? throw new ArgumentNullException(nameof(survey));
        _dvQuestion = dvQuestion ?? throw new ArgumentNullException(nameof(dvQuestion));
        ArgumentNullException.ThrowIfNull(response);

        CreateProfileList(response);
    }

    private void CreateProfileList(Response response)
    {
        // Find the ordinal position of the response in DV question
        int responsePos = _dvQuestion.Responses
            .FindIndex(r => r.RespValue == response.RespValue);

        if (responsePos == -1)
        {
            throw new ArgumentOutOfRangeException(
                $"Response {response.RespValue} is not found in Question {_dvQuestion.QstNumber} as expected. " +
                "This should never happen and is a bug. Please contact Porpoise Support.");
        }

        if (_survey.Data == null)
            throw new InvalidOperationException("Survey data is required for profile analysis.");

        var profileList = new List<ProfileItem>();

        foreach (var ivQuestion in _survey.QuestionList.Where(q => q.VariableType == QuestionVariableType.Independent))
        {
            // Skip the DV question itself if it appears as IV
            if (ivQuestion.Id == _dvQuestion.Id)
                continue;

            var crosstab = new Crosstab(_survey.Data, _dvQuestion, ivQuestion, showCount: false, onProfileTab: true);
            var percentages = crosstab.GetProfilePercentages(responsePos);

            if (percentages != null)
                profileList.AddRange(percentages);
        }

        // Sort by percentage difference descending (biggest drivers first)
        Percentages.AddRange(profileList.OrderByDescending(x => x.PercDiff));
    }
}
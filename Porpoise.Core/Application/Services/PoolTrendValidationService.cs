// Porpoise.Core/Services/PoolTrendValidationService.cs
public static class PoolTrendValidationService
{
    public static List<string> ValidateDvConsistency(List<Question> selectedDvs)
    {
        var errors = new List<string>();

        if (selectedDvs.Count < 2)
            return errors; // nothing to compare

        var first = selectedDvs[0];

        for (int i = 1; i < selectedDvs.Count; i++)
        {
            var q = selectedDvs[i];

            // Must have same number of responses
            if (q.Responses.Count != first.Responses.Count)
            {
                errors.Add($"Question '{q.QstLabel}' has {q.Responses.Count} responses, but '{first.QstLabel}' has {first.Responses.Count}. All DVs must have the same number of responses.");
                continue;
            }

            // Index types must match exactly
            for (int r = 0; r < q.Responses.Count; r++)
            {
                var resp1 = first.Responses[r];
                var resp2 = q.Responses[r];

                if (resp1.IndexType != resp2.IndexType)
                {
                    errors.Add($"Selected questions are not consistent. The Index Type '{resp1.IndexTypeDesc}' for response '{resp1.Label}' in question '{first.QstLabel}' does not match Index Type '{resp2.IndexTypeDesc}' for response '{resp2.Label}' in question '{q.QstLabel}'. You must select questions with the same number of responses and Index values to pool or trend data.");
                }
            }
        }

        return errors;
    }
}
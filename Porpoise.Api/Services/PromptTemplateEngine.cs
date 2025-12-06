using System.Text.RegularExpressions;

namespace Porpoise.Api.Services
{
    /// <summary>
    /// Simple template engine for replacing {{Variable}} placeholders with values
    /// Supports basic {{#each Collection}} loops for iterating over arrays
    /// </summary>
    public class PromptTemplateEngine
    {
        public string Render(string template, Dictionary<string, object> context)
        {
            var result = template;

            // Process {{#each}} blocks first
            result = ProcessEachBlocks(result, context);

            // Then process simple {{Variable}} replacements
            result = ProcessSimpleReplacements(result, context);

            return result;
        }

        private string ProcessEachBlocks(string template, Dictionary<string, object> context)
        {
            var eachPattern = @"\{\{#each\s+(\w+)\}\}(.*?)\{\{/each\}\}";
            var regex = new Regex(eachPattern, RegexOptions.Singleline);

            return regex.Replace(template, match =>
            {
                var collectionName = match.Groups[1].Value;
                var blockTemplate = match.Groups[2].Value;

                if (!context.ContainsKey(collectionName))
                    return string.Empty;

                var collection = context[collectionName];
                if (collection is not System.Collections.IEnumerable enumerable)
                    return string.Empty;

                var result = new System.Text.StringBuilder();
                foreach (var item in enumerable)
                {
                    if (item == null) continue;

                    var itemContext = new Dictionary<string, object>();
                    
                    // If item is a dictionary, use it directly
                    if (item is Dictionary<string, object> dict)
                    {
                        itemContext = dict;
                    }
                    // Otherwise, reflect properties
                    else
                    {
                        var properties = item.GetType().GetProperties();
                        foreach (var prop in properties)
                        {
                            var key = char.ToLower(prop.Name[0]) + prop.Name.Substring(1); // Convert to camelCase
                            itemContext[key] = prop.GetValue(item) ?? string.Empty;
                        }
                    }

                    result.Append(ProcessSimpleReplacements(blockTemplate, itemContext));
                }

                return result.ToString();
            });
        }

        private string ProcessSimpleReplacements(string template, Dictionary<string, object> context)
        {
            var pattern = @"\{\{(\w+)\}\}";
            var regex = new Regex(pattern);

            return regex.Replace(template, match =>
            {
                var key = match.Groups[1].Value;
                if (context.ContainsKey(key))
                {
                    var value = context[key];
                    return value?.ToString() ?? string.Empty;
                }
                return match.Value; // Keep placeholder if not found
            });
        }
    }
}

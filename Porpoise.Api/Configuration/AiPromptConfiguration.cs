namespace Porpoise.Api.Configuration
{
    public class PromptTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Template { get; set; } = string.Empty;
    }

    public class AiPromptConfiguration
    {
        public PromptTemplate QuestionAnalysis { get; set; } = new();
        public PromptTemplate CrosstabAnalysis { get; set; } = new();
    }
}

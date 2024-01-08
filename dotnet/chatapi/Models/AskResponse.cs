namespace Grafedi.LLM.ChatAPI.Models;

public class AskResponse
{
    public string UserInput { get; set; } = string.Empty;
    public string CompletionText { get; set; } = string.Empty;
    public int CompletionTokens { get; set; }
    public int PromptTokens { get; set; }
    public int TotalTokens { get; set; }
    public List<CompletionCitation> Citations { get; set; } = new List<CompletionCitation>();
}

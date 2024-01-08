namespace Grafedi.LLM.ChatAPI.Models;

public class AskRequest
{
    public string UserInput { get; set; } = string.Empty;
    public string? PromptOverride { get; set; }
}

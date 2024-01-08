namespace Grafedi.LLM.ChatAPI.Models;
public class CompletionCitation
{
    public string? Source { get; set; }

    public double Relevance { get; set; }

    public string DocumentId { get; set; } = string.Empty;

    public int DocumentUrlId { get; set; }
}

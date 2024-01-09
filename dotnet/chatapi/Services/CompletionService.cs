// Copyright (c) Microsoft. All rights reserved.

#pragma warning disable SKEXP0011, SKEXP0031, SKEXP0052, SKEXP0003
using System.Text;
using Grafedi.LLM.ChatAPI.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Newtonsoft.Json.Linq;
using TiktokenSharp;

namespace Grafedi.LLM.ChatAPI.Services;
public interface ICompletionService
{
    Task<AskResponse> AskQuestion(AskRequest request);
}

public class CompletionService : ICompletionService
{
    private readonly ISemanticKernelService _semanticKernelService;

    public CompletionService(ISemanticKernelService semanticKernelService)
    {
        this._semanticKernelService = semanticKernelService;
    }

    public async Task<AskResponse> AskQuestion(AskRequest request)
    {
        List<CompletionCitation> citations = new();
        StringBuilder sb = new();

        if (this._semanticKernelService.Kernel == null)
        {
            return new AskResponse();
        }


        int totalTokens = 0;
        if (request.UserInput != null && this._semanticKernelService.Memory != null)
        {
            await foreach (MemoryQueryResult memoryResult in this._semanticKernelService.Memory.SearchAsync("grafedi-paytax-us", request.UserInput, limit: 10, minRelevanceScore: 0.82))
            {
                totalTokens += this.GetTokenCount(memoryResult.Metadata.Text);

                if (totalTokens > 2500)
                {
                    break;
                }

                string additionalInformationText = $"Additional information: {memoryResult.Metadata.Text}";
                sb.AppendLine(additionalInformationText);

                CompletionCitation completionCitation = new() { Relevance = memoryResult.Relevance, DocumentId = memoryResult.Metadata.Id };

                if (memoryResult.Metadata.AdditionalMetadata != null)
                {
                    JObject jo = JObject.Parse(memoryResult.Metadata.AdditionalMetadata);
                    completionCitation.Source = jo.GetValue("source", StringComparison.OrdinalIgnoreCase)?.ToString();

                    string? documentUrlId = jo.GetValue("documentUrlId", StringComparison.OrdinalIgnoreCase)?.ToString();
                    if (documentUrlId != null && int.TryParse(documentUrlId, out int id))
                    {
                        completionCitation.DocumentUrlId = id;
                    }

                    if (string.IsNullOrEmpty(completionCitation.Source) == false)
                    {
                        string s = $"Citation: {completionCitation.Source}";
                        sb.AppendLine(s);
                    }

                    citations.Add(completionCitation);
                }
            }
        }

        // sb.AppendLine("if you live in New Jersey and work in New York, you are required to pay New York income tax. As a non-resident working in New York, you must file a non-resident tax return (Form IT-203) with the state of New York. This is because both New York and New Jersey require that you pay state income taxes in the state where you earn your income.\n\nHowever, when you file your tax return in New Jersey (as a resident), you can claim a credit for the taxes paid to New York, which helps to prevent double taxation of the same income. It's important to file your New York tax return first so you can accurately claim this credit on your New Jersey tax return");
        string addtionalInformation = sb.ToString();
        string skPrompt = string.Empty;

        if (string.IsNullOrEmpty(request.PromptOverride) == false)
        {
            skPrompt = request.PromptOverride;
        }
        else
        {
            skPrompt = await File.ReadAllTextAsync("./Prompts/PayrollAdmin.txt").ConfigureAwait(false);
        }

        totalTokens = this.GetTokenCount(skPrompt);

        KernelFunction chatFunction = this._semanticKernelService.Kernel.CreateFunctionFromPrompt(skPrompt, new OpenAIPromptExecutionSettings { MaxTokens = 5000, Temperature = 0 });
        KernelArguments arguments = new(new Dictionary<string, object?> { { "userInput", request.UserInput }, { "additonalInformation", addtionalInformation } });
        FunctionResult bot_answer = await chatFunction.InvokeAsync(this._semanticKernelService.Kernel, arguments).ConfigureAwait(false);

        AskResponse response = new() { CompletionText = bot_answer.ToString(), Citations = citations };
        this.GetUsageInfo(bot_answer, ref response);

        if (request.UserInput != null)
        {
            response.UserInput = request.UserInput;
        }

        return response;
    }

    private void GetUsageInfo(FunctionResult result, ref AskResponse response)
    {
        if (result.Metadata != null)
        {
            Azure.AI.OpenAI.CompletionsUsage? usage = result.Metadata["Usage"] as Azure.AI.OpenAI.CompletionsUsage;
            if (usage != null)
            {
                response.CompletionTokens = usage.CompletionTokens;
                response.PromptTokens = usage.PromptTokens;
                response.TotalTokens = usage.TotalTokens;
            }
        }
    }
    private int GetTokenCount(string text)
    {
        TikToken tikToken = TikToken.EncodingForModel("gpt-3.5-turbo");
        List<int> i = tikToken.Encode(text); //[15339, 1917]
        return i.Count;
    }

}

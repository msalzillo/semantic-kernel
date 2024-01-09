// Copyright (c) Microsoft. All rights reserved.

#pragma warning disable SKEXP0011, SKEXP0031, SKEXP0052, SKEXP0003
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Pinecone;
using Microsoft.SemanticKernel.Memory;

namespace Grafedi.LLM.ChatAPI.Services;

public interface ISemanticKernelService
{
    ISemanticTextMemory? Memory { get; }
    Kernel? Kernel { get; }
}

public class SemanticKernelService : ISemanticKernelService
{
    private const string Endpoint = "https://grafedi-openai.openai.azure.com/";
    private const string ApiKey = "b993eb427a574b859e1d77adc6c4ba0e";
    // private const string Deployment = "grafedi-openai-tester";
    private const string Deployment = "grafedi-16k";
    // private const string Deployment = "grafedi-instruct";
    private const string DeploymentEmbedding = "grafedi-embedding";

    private const string PineconeEnvironment = "eastus-azure";
    private const string PineconeEnvironmentKey = "68ba380b-dcce-416f-ab9d-39126267bd84";

    private const string EmbeddingModel = "text-embedding-ada-002";

    private readonly Kernel _kernel;
    private readonly PineconeMemoryStore _pineconeMemoryStore;
    private readonly ISemanticTextMemory _memory;
    private readonly OpenAIPromptExecutionSettings _executionSettings;

    public SemanticKernelService()
    {
        this._kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(Deployment, Endpoint, ApiKey)
            .Build();

        this._executionSettings = new()
        {
            MaxTokens = 2000,
            Temperature = 0.0,
            TopP = 0.5
        };

        this._pineconeMemoryStore = new(PineconeEnvironment, PineconeEnvironmentKey);
        MemoryBuilder memoryBuilder = new();
        memoryBuilder.WithAzureOpenAITextEmbeddingGeneration(DeploymentEmbedding, Endpoint, ApiKey, EmbeddingModel);
        memoryBuilder.WithMemoryStore(this._pineconeMemoryStore);

        this._memory = memoryBuilder.Build();
    }

    public ISemanticTextMemory? Memory => this._memory;

    public Kernel? Kernel => this._kernel;
}

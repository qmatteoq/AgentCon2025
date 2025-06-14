namespace OrchestrationSamples;

using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;

public static class KernelCreator
{
    public static Kernel CreateKernel(bool useAzureOpenAI, bool useAzureAISearch = false)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets("4d2b094c-5e54-4666-802e-e69e60da7e76")
            .Build();

        string apiKey = configuration["AzureOpenAI:ApiKey"];
        string deploymentName = configuration["AzureOpenAI:DeploymentName"];
        string endpoint = configuration["AzureOpenAI:Endpoint"];

        string openAIKey = configuration["OpenAI:ApiKey"];
        string openAIModel = configuration["OpenAI:Model"];

        IKernelBuilder kernelBuilder;

        if (useAzureOpenAI)
        {
            kernelBuilder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(deploymentName, endpoint, apiKey);

        }
        else
        {
            kernelBuilder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(openAIModel, openAIKey);
        }

        if (useAzureAISearch)
        {
            string searchEndpoint = configuration["AzureAISearch:Endpoint"];
            string searchApiKey = configuration["AzureAISearch:ApiKey"];
            kernelBuilder.Services.AddAzureAISearchVectorStore(new Uri(searchEndpoint),
             new AzureKeyCredential(searchApiKey));
        }

        return kernelBuilder
                .Build();
    }
}


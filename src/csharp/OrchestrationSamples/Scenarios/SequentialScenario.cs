using Azure.AI.Agents.Persistent;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;

namespace OrchestrationSamples.Scenarios;

public class SequentialScenario : BaseAgent
{

    private SequentialOrchestration orchestration;

    public SequentialScenario()
    {
        var configuration = new ConfigurationBuilder()
           .AddUserSecrets("4d2b094c-5e54-4666-802e-e69e60da7e76")
           .Build();

        string aiFoundryEndpoint = configuration["AzureAIProject:Endpoint"];

        PersistentAgentsClient client = AzureAIAgent.CreateAgentsClient(aiFoundryEndpoint, new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ExcludeVisualStudioCredential = true,
            ExcludeEnvironmentCredential = true,
            ExcludeManagedIdentityCredential = true,
            ExcludeVisualStudioCodeCredential = true
        }));

        var agent = client.Administration.GetAgent("asst_0NygSS1laeWYwlDojM00HUus");

        AzureAIAgent researcherAgent = new AzureAIAgent(agent, client)
        {
            Name = "ResearcherAgent",
            Description ="Conducts research on a given topic and provides a summary."
        };

        // ChatCompletionAgent researcherAgent = new ChatCompletionAgent
        // {
        //     Name = "ResearcherAgent",
        //     Description = "Conducts research on a given topic and provides a summary.",
        //     Instructions = "Gather information on the topic, analyze it, and provide a concise summary highlighting key points.",
        //     Kernel = KernelCreator.CreateKernel(useAzureOpenAI: true)
        // };

        ChatCompletionAgent summarizerAgent = new ChatCompletionAgent
        {
            Name = "SummarizerAgent",
            Description = "Summarizes the findings from the research conducted by the ResearcherAgent.",
            Instructions = "Create a brief summary of the research findings, focusing on the most important aspects and conclusions.",
            Kernel = KernelCreator.CreateKernel(useAzureOpenAI: true)
        };

        ChatCompletionAgent qaAgent = new ChatCompletionAgent
        {
            Name = "QnAAgent",
            Description = "Generates a Q&A document based on the summary provided by the SummarizerAgent.",
            Instructions = "Create a set of questions and answers that cover the key points from the research summary, ensuring clarity and relevance.",
            Kernel = KernelCreator.CreateKernel(useAzureOpenAI: true)
        };

        orchestration = new(researcherAgent, summarizerAgent, qaAgent)
        {
            ResponseCallback = (response) =>
            {
                Console.WriteLine($"\n# RESPONSE: {response}");
                history.Add(response);
                return ValueTask.CompletedTask;
            }
        };
    }

    public async Task RunScenarioAsync(string prompt)
    {
        InProcessRuntime runtime = new InProcessRuntime();
        await runtime.StartAsync();

        var result = await orchestration.InvokeAsync(
            prompt,
            runtime);

        string output = await result.GetValueAsync();
        Console.WriteLine($"\n# RESULT: {output}");
        Console.WriteLine("\n\nORCHESTRATION HISTORY");
        foreach (ChatMessageContent message in history)
        {
            WriteAgentChatMessage(message);
        }
    }
}
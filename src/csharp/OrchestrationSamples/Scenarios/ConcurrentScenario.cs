using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;

namespace OrchestrationSamples.Scenarios;

public class ConcurrentScenario : BaseAgent
{
    private ConcurrentOrchestration orchestration;

    public ConcurrentScenario()
    {
        ChatCompletionAgent sloganAgent = new ChatCompletionAgent
        {
            Name = "SloganAgent",
            Description = "Generates catchy slogans for products or services.",
            Instructions = "Create a short and memorable slogan that captures the essence of the product or service.",
            Kernel = KernelCreator.CreateKernel(useAzureOpenAI: true)
        };

        ChatCompletionAgent logoAgent = new ChatCompletionAgent
        {
            Name = "LogoAgent",
            Description = "Designs logos based on product descriptions.",
            Instructions = "Create the description of a logo that visually represents the product or service, focusing on its key attributes.",
            Kernel = KernelCreator.CreateKernel(useAzureOpenAI: true)
        };

        ChatCompletionAgent timelineAgent = new ChatCompletionAgent
        {
            Name = "TimelineAgent",
            Description = "Creates timelines for product development or project management.",
            Instructions = "Outline a timeline with key milestones and deadlines for the development of the product or service, ensuring clarity and feasibility.",
            Kernel = KernelCreator.CreateKernel(useAzureOpenAI: true)
        };

        orchestration = new(sloganAgent, logoAgent, timelineAgent);
    }

    public async Task RunScenarioAsync(string prompt)
    {
        InProcessRuntime runtime = new InProcessRuntime();
        await runtime.StartAsync();

        var result = await orchestration.InvokeAsync(
            prompt,
            runtime);

        string[] output = await result.GetValueAsync(TimeSpan.FromSeconds(20));
        Console.WriteLine($"# RESULT:\n{string.Join("\n\n", output.Select(text => $"{text}"))}");
    }
}

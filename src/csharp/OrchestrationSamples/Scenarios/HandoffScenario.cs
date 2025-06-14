using System;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.Handoff;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using OrchestrationSamples.Plugins;

namespace OrchestrationSamples.Scenarios;

public class HandoffScenario: BaseAgent
{
    private HandoffOrchestration orchestration;

    public HandoffScenario()
    {
        ChatCompletionAgent triageAgent = new ChatCompletionAgent
        {
            Name = "TriageAgent",
            Description = "This agent is responsible for triaging support requests from employees and directing them to the appropriate agent based on the request type.",
            Instructions = "You are an agent specialized in doing the triage of support requests from employees. Depending on the request, you will hand it off to the most appropriate agent.",
            Kernel = KernelCreator.CreateKernel(true)
        };

        ChatCompletionAgent itAgent = new ChatCompletionAgent
        {
            Name = "ITAgent",
            Description = "This agent is responsible for handling IT-related support requests from employees.",
            Instructions = "You are an agent specialized in IT support. You will help employees with their IT-related issues. You'll do your best to provide suggestions to fix the issue. If you can't fix the issue or the user says that the problem is not solved, you will send a ticket to the IT support team using the TicketPlugin.",
            Kernel = KernelCreator.CreateKernel(true)
        };

        itAgent.Kernel.ImportPluginFromType<TicketPlugin>();

        ChatCompletionAgent hrAgent = new ChatCompletionAgent
        {
            Name = "HRAgent",
            Description = "This agent is responsible for handling HR-related support requests from employees.",
            Instructions = "You are an agent specialized in HR support. You will help employees with their HR-related issues. You'll do your best to provide suggestions to fix the issue.",
            Kernel = KernelCreator.CreateKernel(true)
        };

        var handoffs = OrchestrationHandoffs.StartWith(triageAgent)
                .Add(triageAgent, itAgent, hrAgent)
                .Add(itAgent, triageAgent, "Transfer to this agent if the request is not related to IT support")
                .Add(hrAgent, triageAgent, "Transfer to this agent if the request is not related to HR support");

        orchestration = new HandoffOrchestration(handoffs, triageAgent, itAgent, hrAgent)
        {
            InteractiveCallback = () =>
            {
                Console.WriteLine("Please respond to the request:");
                var response = Console.ReadLine();
                return ValueTask.FromResult(new ChatMessageContent(AuthorRole.User, response));
            },
            ResponseCallback = (content) =>
            {
                history.Add(content);
                Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
                return ValueTask.CompletedTask;
            },
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

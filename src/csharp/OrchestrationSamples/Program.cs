Console.WriteLine("Choose a scenario to run: ");
Console.WriteLine("1. Sequential Orchestration");
Console.WriteLine("2. Concurrent Orchestration");
Console.WriteLine("3. Handoff Orchestration");
Console.WriteLine("4. GroupChat Orchestration");
var scenario = Console.ReadLine() ?? "";

switch (scenario)
{
    case "1":
        var sequentialScenario = new OrchestrationSamples.Scenarios.SequentialScenario();
        Console.WriteLine("Write a topic to research:");
        var sequentialPrompt = Console.ReadLine() ?? "";
        await sequentialScenario.RunScenarioAsync(sequentialPrompt);
        break;

    case "2":
        var concurrentScenario = new OrchestrationSamples.Scenarios.ConcurrentScenario();
        Console.WriteLine("Describe a product or service you want to launch:");
        var concurrentPrompt = Console.ReadLine() ?? "";
        await concurrentScenario.RunScenarioAsync(concurrentPrompt);
        break;
    case "3":
        var handoffScenario = new OrchestrationSamples.Scenarios.HandoffScenario();
        Console.WriteLine("Describe a support request:");
        var handoffPrompt = Console.ReadLine() ?? "";   
        await handoffScenario.RunScenarioAsync(handoffPrompt);
        break;
    case "4":
        var groupChatScenario = new SemanticKernel.Agents.Scenarios.GroupChatScenario();
        Console.WriteLine("Enter a topic for the rap battle:");
        var groupChatPrompt = Console.ReadLine() ?? "";
        await groupChatScenario.RunScenarioAsync(groupChatPrompt);
        break;
    default:
        Console.WriteLine("Invalid scenario selected.");
        break;
}

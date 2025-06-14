using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.Chat;
using OrchestrationSamples;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;

namespace SemanticKernel.Agents.Scenarios
{
    public class GroupChatScenario : BaseAgent
    {
        private GroupChatOrchestration orchestration;
        public GroupChatScenario()
        {
            ChatCompletionAgent rapMCAgent = new ChatCompletionAgent
            {
                Name = "MCAgent",
                Description = "This agent is responsible for reviewing rap lyrics in a rap battle and giving them a score.",
                Instructions = "You are a rap MC and your role is to review the rap lyrics in a rap battle and give it a score. Participants in the content will be given a topic and they will need to create a hip hop version of it. You must perform two tasks: 1) When the battle starts, you must introduce the topic, then give the stage to the two rappers and start a round. 2) Only after the two rappers have created the lyrics, you must evaluate them. You're going to give to the each rap lyrics a score between 1 and 10. You must score them separately. The rapper who gets the higher score wins. You aren't allowed to write lyrics on your own and join the rap battle. You can run maximum 2 rounds, then you must declare the winner.",
                Kernel = KernelCreator.CreateKernel(true)
            };

            ChatCompletionAgent eminemAgent = new ChatCompletionAgent
            {
                Name = "EminemAgent",
                Description = "This agent is a rapper who raps in the style of Eminem.",
                Instructions = "You are a rapper and you rap in the style of Eminem. You are participating to a rap battle. You will be given a topic and you will need to create the lyrics and rap about it.",
                Kernel = KernelCreator.CreateKernel(true)
            };

            ChatCompletionAgent kendrickLamarAgent = new ChatCompletionAgent
            {
                Name = "KendrickLamarAgent",
                Description = "This agent is a rapper who raps in the style of Kendrick Lamar.",
                Instructions = "You are a rapper and you rap in the style of Kendrick Lamar. You are participating to a rap battle. You will be given a topic and you will need to create the lyrics and rap about it.",
                Kernel = KernelCreator.CreateKernel(true)
            };

            orchestration = new GroupChatOrchestration(
             new RoundRobinGroupChatManager { MaximumInvocationCount = 6 },
             rapMCAgent, eminemAgent, kendrickLamarAgent
             )
            {
                ResponseCallback = (response) =>
                {
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
}

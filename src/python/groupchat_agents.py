import asyncio
from semantic_kernel.agents import ChatCompletionAgent
from semantic_kernel.connectors.ai.open_ai import AzureChatCompletion
from semantic_kernel.connectors.ai import FunctionChoiceBehavior
from semantic_kernel.agents import GroupChatOrchestration, RoundRobinGroupChatManager
from semantic_kernel.agents.runtime import InProcessRuntime
from semantic_kernel.contents import ChatMessageContent, AuthorRole
from semantic_kernel import Kernel
from dotenv import load_dotenv
import os


# Helper to create a kernel with Azure OpenAI
def create_kernel():
    load_dotenv()
    deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME")
    endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
    api_key = os.getenv("AZURE_OPENAI_API_KEY")
    kernel = Kernel()
    kernel.add_service(
        AzureChatCompletion(
            deployment_name=deployment_name,
            endpoint=endpoint,
            api_key=api_key,
            service_id="agent"
        )
    )
    return kernel

async def main():
    mc_agent = ChatCompletionAgent(
        name="McAgent",
        description="This agent is responsible for reviewing rap lyrics in a rap battle and giving them a score.",
        instructions="You are a rap MC and your role is to review the rap lyrics in a rap battle and give it a score. Participants in the content will be given a topic and they will need to create a hip hop version of it. You must perform two tasks: 1) When the battle starts, you must introduce the topic, then give the stage to the two rappers and start a round. 2) Only after the two rappers have created the lyrics, you must evaluate them. You're going to give to the each rap lyrics a score between 1 and 10. You must score them separately. The rapper who gets the higher score wins. You aren't allowed to write lyrics on your own and join the rap battle. You can run maximum 2 rounds, then you must declare the winner.",
        kernel=create_kernel()
    )
    
    eminem_agent = ChatCompletionAgent(
        name="EminemAgent",
        description="This agent is a rapper who raps in the style of Eminem.",
        instructions="You are a rapper and you rap in the style of Eminem. You are participating to a rap battle. You will be given a topic and you will need to create the lyrics and rap about it.",
        kernel=create_kernel()
    )
    
    kendricklamar_agent = ChatCompletionAgent(
        name="kendrickLamarAgent",
        description="This agent is a rapper who raps in the style of Kendrick Lamar.",
        instructions="You are a rapper and you rap in the style of Kendrick Lamar. You are participating to a rap battle. You will be given a topic and you will need to create the lyrics and rap about it.",
        kernel=create_kernel()
    )
    
    group_chat_orchestration = GroupChatOrchestration(
        members=[mc_agent, eminem_agent, kendricklamar_agent],
        manager=RoundRobinGroupChatManager(max_rounds=6),  
        agent_response_callback=agent_response_callback,
    )
    
    runtime = InProcessRuntime()
    runtime.start()
    
    prompt = input("Describe the topic of the rap battle: ")
    orchestration_result = await group_chat_orchestration.invoke(
        task=prompt,
        runtime=runtime
    )
    
    value = await orchestration_result.get()
    print(f"***** Final Result *****\n{value}")
    
    await runtime.stop_when_idle()
    
def agent_response_callback(message: ChatMessageContent) -> None:
    print(f"**{message.name}**\n{message.content}")
    
if __name__ == "__main__":
    asyncio.run(main())
        
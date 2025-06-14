import asyncio
from semantic_kernel.connectors.ai.open_ai import AzureChatCompletion
from semantic_kernel.agents import AzureAIAgent, SequentialOrchestration, ChatCompletionAgent, AzureAIAgentSettings
from semantic_kernel.agents.runtime import InProcessRuntime
from semantic_kernel import Kernel
from azure.identity.aio import DefaultAzureCredential
from semantic_kernel.contents import ChatMessageContent


from dotenv import load_dotenv
import os


# Helper to create a kernel with Azure OpenAI
def create_kernel():
    deployment_name = os.getenv("AZURE_OPENAI_DEPLOYMENT_NAME")
    endpoint = os.getenv("AZURE_OPENAI_ENDPOINT")
    api_key = os.getenv("AZURE_OPENAI_API_KEY")
    kernel = Kernel()
    kernel.add_service(
        AzureChatCompletion(
            deployment_name=deployment_name,
            endpoint=endpoint,
            api_key=api_key
        )
    )
    return kernel

async def main():
    # Define agents
    load_dotenv()
    async with (
    DefaultAzureCredential() as creds,
    AzureAIAgent.create_client(credential=creds, endpoint=os.getenv("AZURE_AI_AGENT_ENDPOINT")) as client):
                
    
        agent_definition = await client.agents.get_agent(agent_id="asst_0NygSS1laeWYwlDojM00HUus")
        researcher_agent = AzureAIAgent(client=client, definition=agent_definition, name="ResearcherAgent")

        summarizerAgent = ChatCompletionAgent(
            name="SummarizerAgent",
            description="Summarizes the findings from the research conducted by the ResearcherAgent.",
            instructions="Create a brief summary of the research findings, focusing on the most important aspects and conclusions.",
            kernel=create_kernel()
        )
        qaAgent = ChatCompletionAgent(
            name="QAAgent",
            description="Generates a Q&A document based on the summary provided by the SummarizerAgent.",
            instructions="Create a set of questions and answers that cover the key points from the research summary, ensuring clarity and relevance.",
            kernel=create_kernel()
        )

        orchestrator = SequentialOrchestration(members=[researcher_agent, summarizerAgent, qaAgent], agent_response_callback=agent_response_callback)
        
        runtime = InProcessRuntime()
        runtime.start()

        prompt = input("Describe the topic you want to research: ")
        orchestration_result = await orchestrator.invoke(task=prompt, runtime=runtime)
        
        value = await orchestration_result.get(timeout=20)

            
        await runtime.stop_when_idle()

def agent_response_callback(message: ChatMessageContent) -> None:
    print(f"# {message.name}\n{message.content}")
    print()

if __name__ == "__main__":
    asyncio.run(main())
        
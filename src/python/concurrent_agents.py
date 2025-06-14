import asyncio
from semantic_kernel.agents import ChatCompletionAgent
from semantic_kernel.connectors.ai.open_ai import AzureChatCompletion
from semantic_kernel.agents import ConcurrentOrchestration
from semantic_kernel.agents.runtime import InProcessRuntime
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
            api_key=api_key
        )
    )
    return kernel

async def main():
    # Define agents
    slogan_agent = ChatCompletionAgent(
        name="SloganAgent",
        description="Generates catchy slogans for products or services.",
        instructions="Create a short and memorable slogan that captures the essence of the product or service.",
        kernel=create_kernel()
    )
    logo_agent = ChatCompletionAgent(
        name="LogoAgent",
        description="Designs logos based on product descriptions.",
        instructions="Create the description of a logo that visually represents the product or service, focusing on its key attributes.",
        kernel=create_kernel()
    )
    timeline_agent = ChatCompletionAgent(
        name="TimelineAgent",
        description="Creates timelines for product development or project management.",
        instructions="Outline a timeline with key milestones and deadlines for the development of the product or service, ensuring clarity and feasibility.",
        kernel=create_kernel()
    )

    # Orchestrate agents concurrently
    orchestrator = ConcurrentOrchestration(members=[slogan_agent, logo_agent, timeline_agent])

    runtime = InProcessRuntime()
    runtime.start()

    prompt = input("Describe the product or service you want to launch: ")
    orchestration_result = await orchestrator.invoke(task=prompt, runtime=runtime)

    value = await orchestration_result.get(timeout=20)
    # For the concurrent orchestration, the result is a list of chat messages
    for item in value:
        print(f"# {item.name}: {item.content}")
        
    await runtime.stop_when_idle()

if __name__ == "__main__":
    asyncio.run(main())
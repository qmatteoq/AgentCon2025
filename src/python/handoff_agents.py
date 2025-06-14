import asyncio
from semantic_kernel.agents import ChatCompletionAgent
from semantic_kernel.connectors.ai.open_ai import AzureChatCompletion
from semantic_kernel.connectors.ai import FunctionChoiceBehavior
from semantic_kernel.agents import HandoffOrchestration, OrchestrationHandoffs
from semantic_kernel.agents.runtime import InProcessRuntime
from semantic_kernel.contents import ChatMessageContent, AuthorRole
from semantic_kernel import Kernel
from dotenv import load_dotenv
import os

from plugins.ticket_plugin import TicketPlugin

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
    # Define agents
    triageAgent = ChatCompletionAgent(
        name="TriageAgent",
        description="This agent is responsible for triaging support requests from employees and directing them to the appropriate agent based on the request type",
        instructions="You are an agent specialized in doing the triage of support requests from employees. Depending on the request, you will hand it off to the most appropriate agent.",
        kernel=create_kernel()
    )

    it_agent_kernel= create_kernel()
    settings = it_agent_kernel.get_prompt_execution_settings_from_service_id(service_id="agent")
    settings.function_choice_behavior = FunctionChoiceBehavior.Auto()
    
    it_agent_kernel.add_plugin(TicketPlugin(), plugin_name="TicketPlugin")
    
    it_agent = ChatCompletionAgent(
        name="ITAgent",
        description="This agent is responsible for handling IT-related support requests from employees",
        instructions="You are an agent specialized in IT support. You will help employees with their IT-related issues. You'll do your best to provide suggestions to fix the issue. If you can't fix the issue or the user says that the problem is not solved, you will send a ticket to the IT support team using the TicketPlugin.",
        kernel=it_agent_kernel
    )
    hr_agent = ChatCompletionAgent(
        name="HrAgent",
        description="This agent is responsible for handling HR-related support requests from employees.",
        instructions="You are an agent specialized in HR support. You will help employees with their HR-related issues. You'll do your best to provide suggestions to fix the issue",
        kernel=create_kernel()
    )
    
    handoffs = (
        OrchestrationHandoffs()
        .add_many(    # Use add_many to add multiple handoffs to the same source agent at once
            source_agent=triageAgent.name,
            target_agents={
                hr_agent.name: "Transfer to this agent if the issue is HR related",
                it_agent.name: "Transfer to this agent if the issue is IT related"
            },
        )
        .add(    # Use add to add a single handoff
            source_agent=hr_agent.name,
            target_agent=triageAgent.name,
            description="Transfer to this agent if the issue is not HR related",
        )
        .add(
            source_agent=it_agent.name,
            target_agent=triageAgent.name,
            description="Transfer to this agent if the issue is not IT related",
        )
    )
    
    handoff_orchestration = HandoffOrchestration(
        members=[
            triageAgent,
            it_agent,
            hr_agent,
        ],
        handoffs=handoffs,
        agent_response_callback=agent_response_callback,
        human_response_function=human_response_function,
    )
    
    runtime = InProcessRuntime()
    runtime.start() 
    
    prompt = input("Describe the support you need: ")
    orchestration_result = await handoff_orchestration.invoke(
        task=prompt,
        runtime=runtime,
    )
    
    value = await orchestration_result.get()
    print(value)
    
    await runtime.stop_when_idle()
    
def agent_response_callback(message: ChatMessageContent) -> None:
    print(f"{message.name}: {message.content}")
    
def human_response_function() -> ChatMessageContent:
    user_input = input("User: ")
    return ChatMessageContent(role=AuthorRole.USER, content=user_input)

if __name__ == "__main__":
    asyncio.run(main())
    

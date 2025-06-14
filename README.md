# AgentCon 2025 - Milan

AgentCon2025 is a sample project demonstrating advanced orchestration and agent-based scenarios using both C# and Python. The project showcases various agent orchestration patterns, plugins, and scenarios, and is designed for educational and demonstration purposes at the AgentCon 2025 event in Milan.

## Project Structure

```
AgentCon2025/
├── docs/                  # Documentation and resources
├── src/
│   ├── csharp/            # C# implementation
│   │   ├── OrchestrationSamples/   # Core orchestration logic and scenarios
│   │   │   ├── Plugins/            # C# plugins for agent scenarios
│   │   │   ├── Scenarios/          # Example agent orchestration scenarios
│   │   └── Sequential/             # Additional C# scenarios/plugins
│   └── python/            # Python implementation
│       ├── plugins/       # Python plugins for agent scenarios
│       └── ...            # Python scenario scripts
├── AgentCon2025.sln       # Solution file
├── README.md              # Project overview (this file)
├── LICENSE                # License information
├── CODE_OF_CONDUCT.md     # Code of conduct
├── SECURITY.md            # Security policy
├── SUPPORT.md             # Support guidelines
```

## Features

- **Multi-language support:** C# and Python implementations for agent orchestration.
- **Sample Scenarios:** Includes concurrent, group chat, handoff, and sequential agent scenarios.
- **Plugin Architecture:** Easily extend agent capabilities via plugins (C# and Python).
- **Azure Integration:** Uses Azure SDKs for AI, storage, and search (see C# dependencies).

## Getting Started

1. **Clone the repository:**
   ```bash
   git clone <repo-url>
   cd AgentCon2025
   ```
2. **C# (dotnet) setup:**
   - Open `src/csharp/AgentCon2025.slnx`s in Visual Studio or VS Code.
   - Restore NuGet packages and build the solution.
   - Run the desired scenario from the `OrchestrationSamples` project.s
3. **Python setup:**
   - Navigate to `src/python/`.
   - Install dependencies (if any) and run the scenario scripts, e.g.:
     ```bash
     python concurrent_agents.py
     ```


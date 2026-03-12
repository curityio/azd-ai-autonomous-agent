# Dev Containers

The goal of this project is to securely connect internet users to backend environments.  
Instead of dev containers we currently provide a [Local End-to-End Setup](../docs/DEVELOPMENT.md).

## Development Backend

Developers run the following backend components:

- A [Docker Compose Network](../tools/local/docker-compose.yml) that runs identity components.
- The [A2A Server](../src/AutonomousAgent) and [MCP Server](../src/PortfolioMcpServer) run on the local computer, with fast feedback on C# code changes.

## Internet Client

Developers run the following frontend component that uses tokens to connect to backend components:

- The [Console Client](../src/ConsoleClient) runs on the local computer, with fast feedback on C# code changes.

## End-to-End Flow

The use of the local computer enables an interactive user flow with understandable connections.

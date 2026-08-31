# Development

The azd template provides a local deployment, to show how to get started with secure agent development.

## Application Components

An organization would use .NET to develop high-level application components:

- The [Autonomous Agent](../src/AutonomousAgent/README.md) is an A2A server that integrates with an Azure LLM.
- The [Portfolio MCP Server](../src/PortfolioMcpServer/README.md) is a resource server that the LLM instructs the agent to call.
- The [Internet Application](../src/ConsoleClient/README.md) is any app that runs an A2A client and sends access tokens.

## Agent Development

To support agent development, run the example Docker backend environment:

```bash
./tools/local/backend.sh
```

Docker provides the following backend components:

- External API gateway: `http://localhost`.
- Internal API gateway: performs token exchange when the agent calls the MCP server.
- Portfolio MCP Server external URL: `http://localhost/portfolio-mcp-server`.
- Curity Identity Server OAuth Endpoints: `http://localhost:8443`.
- Curity Identity Server Admin Endpoints: `http://localhost:6749/admin`.

Run the anonymous agent locally, which calls the deployed MCP server:

```bash
./src/AutonomousAgent/run.sh
```

Run the console client to sign in and get an access token with which to call the Anonymous Agent:

```bash
./src/ConsoleClient/run.sh
```

The local computer deployment uses the simplest form of authenticator, where you only enter a username.  
You can enter any value to quickly get an access token for the local computer environment.

<img src="images/username-authenticator.png" alt="Username Authenticator" style="width:50%;" />

The minimal client then calls the autonomous agent with a naural language command and the access token.  
Wait a few seconds and you will get a report that the Azure LLM produces.

## MCP Server Development

MCP server developers do not have to run local end-to-end flows as part of normal development.  
Instead, they can work on a single component at a time, using test-driven development.  
To demonstrate test-driven development, use the following commands in different terminal windows.  

```bash
./src/PortfolioMcpServer/run.sh
./src/PortfolioMcpServer/test.sh
```

The [OAuth integration tests](../src/PortfolioMcpServer/security-tests/src/SecurityTests.cs) show how to send mock JWT access tokens to the Portfolio MCP Server.  
Developers can control access token payloads and test many access token security conditions:

```text
[xUnit.net 00:00:00.26] SecurityTests: >>> Starting mock authorization server ...
[xUnit.net 00:00:00.47] SecurityTests: >>> Stopping mock authorization server ...
  Passed SecureMcpRequest_GetCurrentStockPrices_SucceedsWithValidAccessToken [178 ms]
  Passed SecureMcpRequest_ListTools_Returns401ForAccessTokenWithInvalidAudience [9 ms]
```

## Microsoft .NET AI Libraries

C# and the following Microsoft AI libraries are used to build the application components.  
As a result, both AI protocol complexity and security protocol complexity are externalized from application code.

- The autonomous AI agent uses the [Microsoft Agent Framework](https://github.com/microsoft/agent-framework), where foundry agents are the most up to date option.  

- To run as an A2A server or make outbound A2A requests, the agent uses the [A2A .NET SDK](https://github.com/a2aproject/a2a-dotnet).

- To make outbound MCP client connections, and to secure inbound A2A, the agent uses the [MCP .NET SDK](https://github.com/modelcontextprotocol/csharp-sdk).  

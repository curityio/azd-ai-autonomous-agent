# Development

On a local computer you can run local end-to-end flows or develop a single component at a time.  

## Application Components

An organization would use .NET to develop high-level application components:

- The [Autonomous Agent](../src/AutonomousAgent/README.md) is an A2A server that integrates with an Azure LLM.
- The [Portfolio MCP Server](../src/PortfolioMcpServer/README.md) is a resource server that the LLM instructs the agent to call.
- The [Internet Application](../src/ConsoleClient/README.md) is any app that runs an A2A client and sends access tokens.

## Docker Components

After deployment, Docker provides the following backend components:

- Autonomous Agent (A2A Server): `http://localhost:3000`
- Portfolio MCP Server: `http://localhost:3001`
- Curity Identity Server OAuth Endpoints: `http://localhost:8443`
- Curity Identity Server Admin Endpoints: `http://localhost:6749/admin`
- An external API gateway at `http://localhost` that exchanges incoming opaque access tokens for downscoped JWTs
- An internal API gateway that audits secure requests from the autonomous agent

Run the console client to sign in and get an access token with which to call the Anonymous Agent:

```bash
./src/ConsoleClient/run.sh
```

The local computer deployment uses the simplest form of authenticator, where you only enter a username.  
You can enter any value to quickly get an access token for the local computer environment.

<img src="images/username-authenticator.png" alt="Username Authenticator" style="width:50%;" />

The minimal client then calls the autonomous agent with a naural language command and the access token.  
Wait a few seconds and you will get a report that the Azure LLM produces.

## Use Test-Driven Development

MCP server developers do not have to run local end-to-end flows as part of normal development.  
Instead, they can work on a single component at a time, using test-driven development.  
To demonstrate test-driven development, first run the MCP server:

```bash
cd src/PortfolioMcpServer
./run.sh
```

In another terminal window, run some integration tests that send mock access tokens:

```bash
cd src/PortfolioMcpServer.Tests
./run.sh
```

The [OAuth security tests](../src/PortfolioMcpServer.Tests/src/SecurityTests.cs) send mock JWT access tokens to the Portfolio MCP Server.  
Developers can productively test all security conditions without needing to authenticate users or agents:

```text
xUnit.net v3 In-Process Runner v4.0.0+8bf043c053 (64-bit .NET 10.0.2)
    [SecurityTests] >> Starting mock access token issuer ...
    [SecurityTests] >> ListTools_Succeds_WithValidAccessToken PASSED ✓
    [SecurityTests] >> GetPortfolio_Succeeds_WithValidAccessToken PASSED ✓
    [SecurityTests] >> GetStocks_Succeeds_WithValidAccessToken PASSED ✓
    [SecurityTests] >> ListTools_Returns401_ForAccessTokenWithInvalidAudience PASSED ✓
    [SecurityTests] >> GetPortfolio_Returns401_ForAccessTokenWithInvalidIssuer PASSED ✓
    [SecurityTests] >> GetStocks_Returns401_ForAccessTokenWithInvalidSigningKey PASSED ✓
    [SecurityTests] >> GetPortfolio_Returns403_ForMissingRequiredScope PASSED ✓
    [SecurityTests] >> GetPortfolio_Returns403_ForMissingAgentClaims PASSED ✓
    [SecurityTests] >> GetStocks_Returns403_ForInvalidAgentDepartment PASSED ✓
    [SecurityTests] >> Stopping mock access token issuer ...
```

## Microsoft .NET AI Libraries

C# and the following Microsoft AI libraries are used to build the application components.  
As a result, both AI protocol complexity and security protocol complexity are externalized from application code.

- The autonomous AI agent uses the [Microsoft Agent Framework](https://github.com/microsoft/agent-framework), where foundry agents are the future-facing option.  

- To run as an A2A server or make outbound A2A requests, the agent uses the [A2A .NET SDK](https://github.com/a2aproject/a2a-dotnet).

- To make outbound MCP client connections, and to secure inbound A2A, the agent uses the [MCP .NET SDK](https://github.com/modelcontextprotocol/csharp-sdk).  

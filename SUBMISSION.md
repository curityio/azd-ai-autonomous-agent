# Submission

The submission to Azure needs to follow the [Contributor Guide](https://azure.github.io/awesome-azd/docs/contribute/).

## Pull Request

We will make a [pull request similar to this one](https://github.com/Azure/awesome-azd/pull/444/changes), with data and a diagram.  

## Data

```json
{
  "title": "Demonstrates Azure AI integration of customer users with enterprise data, using token intelligence",
  "description": "A secured A2A and MCP flow, where customer users send commands to backend agents and resource servers apply token-based authorization. Includes a secure deployment where tokens enable agents to complete complex flows, while resource servers and gateways can apply dynamic access controls.",
  "preview": "./templates/images/autonomous-ai-agent.png",
  "authorUrl": "https://curty.io",
  "author": "Curity AB",
  "source": "https://github.com/curityio/azd-ai-autonomous-agent",
  "tags": [
    "A2A",
    "MCP",
    "OAuth"
    ],
  "languages": [
    "C#"
  ],
  "frameworks": [
    "ASP.Net",
    "A2A SDK",
    "MCP SDK"
  ],
  "azureServices": [
    "Container Apps",
    "Azure AI Foundry",
    "Entra ID",
    "AzureSQL"
  ],
  "IaC": [
    "bicep"
  ],
  "id": "4218b822-4eca-451e-8b9b-c89a9be0dec5"
}
```

## Architecture Diagram

![Architecture](docs/images/initial-technical-flow.png)

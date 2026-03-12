# Secured Autonomous Agent

An azd template to showcase an enterprise AI security architecture with token intelligence.  
Agents can act autonomously, with limits defined by administrators and with runtime human approvals.

Enables customers to run internet applications that integrate with Azure AI Foundry and enterprise data.  
Users can manipulate authorized data in flexible ways, with [rich responses](docs/AI-DATA-REPORTING.md) from the AI model.

```text
Give me a markdown report on the last 3 months of stock transactions and the value of my portfolio
```

[Features](README.md#features) . [Getting Started](README.md#getting-started) . [Guidance](README.md#guidance)

## Important Security Notice

This template, the application code, and configuration, showcase an architecture to protect business data.  
The integration also includes convenient local computer infrastructure connections.

The infrastructure security should be further hardened before deploying to production environments.  
For example, use Azure workload identities for Curity Identity Server database connections.

## Features

The repository demonstrates the following main features:

- C# application code to integrate a backend agent with Azure AI Foundry.
- C# application code to use OpenID Connect to authenticate users and get initial access tokens.
- C# A2A server and MCP server code to validate access tokens and implement token exchange.
- Configuration and deployment of identity systems and API gateways.

## Resources

The code provides 3 applications, developed with Microsoft SDKs:

- A console client serves as a secured internet application that uses A2A to send customer support requests.
- A secured backend agent processes customer support requests and integrates with Azure AI Foundry.
- An MCP server uses optimal access tokens and claims-based authorization to protect enterprise resources.

The resources support multiple deployment scenarios:

- Local Xunit security testing of the MCP server as a standalone component.
- A local deployment to promote token understanding.
- An Azure deployment that can be triggered from the local computer or a GitHub workflow.

## Architecture Diagram

Enterprises build applications with high level productive programming languages.  
Optimal access tokens restrict privileges and can enable dynamic access controls.

![Initial Technical Flow](docs/images/initial-technical-flow.png)

## Getting Started

Use an Azure development account with access to the Azure portal.  
Follow the [Azure AI README](docs/AZURE-AI-SETUP.md) to get connected to Azure LLMs for development in a compliant Azure region.  

### Create a Project

Create a project from the template, and set an initial environment name of `dev` when prompted.  
Check the new project into source control, so that you can configure a GitHub workflow later.

```bash
mkdir my-secure-ai-integration && cd my-secure-ai-integration
azd init --template https://github.com/curityio/azd-ai-autonomous-agent
azd env set AZURE_LOCATION='uksouth'
```

### Local Environment

Use a Windows, macOS or Linux computer with a Linux-based shell (such as Git bash on Windows).  
Install the following local computer tools:

- **Azure CLI** (`az`) - to connect to Azure AI Foundry with an Azure CLI credential
- **Azure Developer CLI** (`azd`) to use higher level commands to manage projects and deploy to Azure
- **.NET SDK 10+** (to build and run C# applications)
- **Docker** (to build custom Docker images for identity components)
- **openssl** (to create runtime secrets)
- **envsubst** (to configure dynamically generated parameter values)
- **jq** (to read JSON in bash scripts)

### Quick Start

The quick start enables you to integrate all C# applications locally, and run an end-to-end flow.  
Log in to the Azure CLI so that the local agent can present a CLI identity to the Azure AI Foundry:

```bash
az login
```

Run a local deployment that runs the agent and MCP server, along with Docker identity infrastructure:

```bash
./tools/local/backend.sh
```

The first time you run a deployment, a CLI uses the browser to sign you in at Curity.  
The CLI then uses an access token to download a trial license for the Curity Identity Server.

Then, run a console application that connects to the local backend.  
When prompted with a login form, enter any username to simulate real user authentication:

```bash
./src/ConsoleClient/run.sh
```

See the [Development README](docs/DEVELOPMENT.md) to learn more about local development behaviors.

## Deployment

This template includes an infrastructure-as-code (IaC) deployment to Azure.   
Continue to use an Azure development account and ensure that it has an Entra ID tenant.  

### Run the Deployment

Log in to the Azure Developer CLI, to use azd deployment commands:

```bash
azd auth login
```

Deploy backend components to the Azure cloud and wait a few minutes for the deployment to complete.  

```bash
azd up
```

### Test the Deployment

Then, re-run the console application, pointing it the Azure backend.  
Sign in with an Entra ID user account and the user's Entra ID user authentication method:

```bash
export A2A_EXTERNAL_URL=$(azd env get-value A2A_EXTERNAL_URL)
./src/ConsoleClient/run.sh
```

### Tear Down the Deployment

Later, when you have finished with the deployment, free resources:

```bash
azd down --purge
```

### Create a Deployment Pipeline

Once you have finished working on deployments locally, create a GitHub workflow:

```bash
azd pipeline config
```

The project includes an [azure-dev.yml file](.github/workflows/azure-dev.yml), to deploy all components.  
Once configured, all future checkins to `main` will trigger Azure deployment upgrades.  

### Further Information

The following documents explain more about deployments, endpoints and troubleshooting.

- [Azure Deployment](docs/AZURE-DEPLOYMENT.md)
- [Azure Endpoints](docs/AZURE-ENDPOINTS.md)
- [GitHub Workflow](docs/GITHUB-WORKFLOW.md)

## Guidance

The deeper behaviors are a future-proof backend AI deployment with security controls.

### API Gateways

- An external gateway delivers downscoped JWT access tokens to agents.
- An internal gateway sits between agents and resource servers, as a pattern to govern agent access.

### Curity Identity Server

- Delivers least-privilege access tokens to agents and other clients, to restrict levels of access.
- Enables resource servers and gateways to use any dynamic token claims, for flexible access control.
- Exchanges tokens so that agents can federate to complete complex tasks.

### Entra ID

A specialist token issuer can integrate with existing identity systems.  
In the example deployment, Entra ID is used for all user account storage and user authentication.

### Learn More

- See the [Token Flow README](docs/TOKEN-FLOW.md) to understand the token details for the customer support use case.
- See the [OAuth Configuration README](docs/OAUTH-CONFIGURATION.md) to understand OAuth security settings.
- See the [Advanced Use Cases README](docs/ADVANCED-USE-CASES.md) for flows to meet other enterprise requirements.

## License

This project is licensed under the [Apache License 2.0](LICENSE.md).
 
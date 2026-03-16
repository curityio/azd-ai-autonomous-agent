# Security

The repository uses both infrastructure security and enterprise data security.

## Infrastructure Security

Azure deployment best practices provide strong Azure infrastructure security:

- [Azure Deployment](docs/AZURE-DEPLOYMENT.md)
- [Azure Endpoints](docs/AZURE-ENDPOINTS.md)
- [GitHub Workflow](docs/GITHUB-WORKFLOW.md)

## Enterprise Data Security

OAuth 2.0 provides future-proof strong security for enterprise data, APIs, applications and users: 

- [OAuth Configuration](docs/OAUTH-CONFIGURATION.md)
- [Token Flow](docs/TOKEN-FLOW.md)
- [Advanced Use Cases](docs/ADVANCED-USE-CASES.md)

## Managed Identities

The deployment uses the following managed identities:

- The GitHub workflow uses a managed identity to run Azure deployments
- The Autonomous Agent uses a managed identity to connect to Azure AI Foundry

## Password Credentials

For developer convenience, and to reduce scope / complexity, some connections use simple passwords.  
Harden at least the first of these connections for production deployments.  

- The JDBC connection from the Curity Identity Server to Azure SQL could use a managed identity and strict firewall rules.
- The Admin UI for the Curity Identity Server could use an Entra ID federated login.
- Token exchange could use Azure JWT client assertions instead of client secrets.

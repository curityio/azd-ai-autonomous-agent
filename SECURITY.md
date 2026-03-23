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

## Further Security Hardening

For developer convenience, and to reduce scope / complexity, some connections do not use the strongest security.  
For production deployments, first tighten firewall rules:  

- Azure SQL connections.
- Azure AI Foundry project connections.

Also consider using stronger credentials for these secrets:

- The JDBC connection from the Curity Identity Server to Azure SQL could use a managed identity.
- The Admin UI for the Curity Identity Server could use an Entra ID federated login.
- Token exchange could use JWT workload identities instead of client secrets.

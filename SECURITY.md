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
Harden at least the first of these connections for production deployments.  

- Azure SQL connections could require managed identities and could use stricter firewall rules.
- The Azure AI Foundry could require stricter firewall rules.
- The Admin UI for the Curity Identity Server could use an Entra ID federated login.
- Token exchange could use JWT workload identities instead of client secrets.

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

The main focus of this azd template is correct use of access tokens to ensure end-to-end security.  
To enable developer connections, and to reduce scope / complexity, some connections do not use the strongest security.  

For production deployments, you should first tighten firewall rules:  

- Azure SQL connections.
- Azure AI Foundry project connections.

Also aim to use stronger credentials for the following connections:

- The Curity Identity Server could use a [Passwordless JDBC Connection to Azure SQL](https://learn.microsoft.com/en-us/azure/azure-sql/database/authentication-aad-overview).
- The Admin UI for the Curity Identity Server could use an [Entra ID federated login](https://curity.io/resources/learn/federated-login-to-admin-ui/).
- In supporting environments, token exchange could use [JWT workload identities](https://curity.io/resources/learn/oauth-client-credentials-kubernetes/) instead of client secrets.

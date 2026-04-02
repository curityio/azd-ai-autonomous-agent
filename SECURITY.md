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

## User Accounts

The example deployment uses passkeys as its default authentication method and creates active user accounts after entry of an email.  
This is a testing convenience and you can change the [OAuth Configuration](docs/OAUTH-CONFIGURATION.md) to manage users in alternative ways.

A real deployment that stores user accounts in the Curity Identity Server would typically use a registration authenticator.  
Accounts would then be created in an inactive state, and the user could perform an action like email verification to activate the account.  
Signup flows also typically present forms to collect user attributes.

## Email Verification

The example deployment uses the [maildev SMTP test server](https://github.com/maildev/maildev) to simulate email verification when the user creates a passkey.  
Maildev allows you to access email inboxes as any test user, whereas a real SMTP server only allows users to access their own emails.  
A production deployment must replace the mock SMTP server with a real SMTP server.

## Managed Identities

The deployment uses the following managed identities:

- The GitHub workflow uses a managed identity to run Azure deployments
- The Autonomous Agent uses a managed identity to connect to Azure AI Foundry

## Connection Hardening

For developer convenience, and to reduce scope / complexity, some backend connections do not use the strongest security.  
For production deployments, first tighten firewall rules:  

- Azure SQL connections.
- Azure AI Foundry project connections.

Also consider using stronger credentials for these secrets:

- The JDBC connection from the Curity Identity Server to Azure SQL could use a managed identity.
- The Admin UI for the Curity Identity Server could use an Entra ID federated login.
- Token exchange could use JWT workload identities instead of client secrets.

# Tasks

## LLM Routing

- Route the LLM request via the internal gateway.

## Deployment Updates

- Set the AZURE_CLIENT_ID property for my deployed container apps.
- Change plugin folder layout and update deployment references.
- Rename the agent token exchange client to an internal gateway client.

## LLM Token Validation

- Do introspection via token exchange, return unauthorized if required, then remove the Authorization header.

## Internal Token Exchange

- Get the client ID from the workload identity and use it in a token exchange request.
- Consider validating the workload credential using the Entra URL for the tenant.
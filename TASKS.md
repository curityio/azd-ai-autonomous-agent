# Tasks

## Deployment Simplification

- Remove complex routing
- Revamp MCP server tests

## LLM Token Validation

- Route the LLM request via the internal gateway.
- Add a validate only setting and remove the Authorization header.

## Workload Identity Validation

- Do a separate internal gateway plugin
- Use the sub claim as an example agent attribute
- Get an audience claim for the gateway
- Validate using issuer signing keys

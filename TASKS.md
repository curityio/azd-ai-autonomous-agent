# Tasks

## AI Logic Updates

- Use the OpenAI responses API
- Simplify agent creation code if possible
- Use low reasoning for faster responses
- Return current stock prices in another MCP tool

## LLM Token Validation

- Route the LLM request via the internal gateway.
- Add a validate only setting and remove the Authorization header.

## Workload Identity Validation

- Do a separate internal gateway plugin
- Use the sub claim as an example agent attribute
- Get an audience claim for the gateway
- Validate using issuer signing keys

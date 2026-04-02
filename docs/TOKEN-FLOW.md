# Initial Token Flow

Each backend component receives optimal tokens with security context, to enable correct business authorization.

![Initial Technical Flow](images/initial-technical-flow.png)

## Initial Access Token (AT1)

The external A2A client receives an opaque access token of the following form.  
This access token format prevents internet clients from reading potentially sensitive access token payloads.

```text
_0XBPWQQ_2fb1bc61-0e98-413c-a44d-d8a46d3bd2f2
```

The underlying token claims would typically be those of a customer support application.  
In the example deployment these are `openid stocks/read`.  
In many use cases, the customer support application could have multiple scopes that the agent should not have access to.

## Agent Access Token (AT2)

The external gateway uses [Token Exchange](https://curity.io/resources/learn/token-exchange-flow/) to reduce the scopes of the access token to `stocks/read`.  
The token exchange also converts the format of the incoming access token to a JWT.  

```json
{
  "jti": "152bd3df-7b86-4701-9514-a117cde86165",
  "delegationId": "cd7119d8-f7bc-4d61-b296-2b117e641bb9",
  "exp": 1772021039,
  "nbf": 1772020139,
  "scope": "stocks/read",
  "iss": "http://localhost:8443/oauth/v2/oauth-anonymous",
  "sub": "934d737304b1bbc5cc0d443749e64a473211cb5af9e88b069abbd0ed728741b9",
  "aud": "https://agent.demo.example",
  "iat": 1772020139,
  "purpose": "access_token",
  "customer_id": "178",
  "region": "USA",
  "client_id": "console-client"
}
```

The agent validates the JWT before triggering any operations that invoke the Azure LLM.  
The agent only accepts tokens with an audience restriction of `https://agent.demo.example` and requires a scope of `stocks/read`.

## Portfolio MCP Server Access Token (AT3)

The autonomous agent then runs its own token exchange, to get a token to send to the Portfolio API.  
The new token includes the agent identity and an audience that the Portfolio API accepts.  

The Portfolio MCP Server receives the following access token payload.  
The Portfolio MCP Server only accepts tokens with an audience restriction of `https://mcp.demo.example` and a scope of `stocks/read`.

```json
{
  "jti": "fc7fbe2a-27d1-4a95-ab05-5a95bd236a07",
  "delegationId": "2818695a-949a-4622-b5cb-9c1a9ba49716",
  "exp": 1771434954,
  "nbf": 1771434054,
  "scope": "stocks/read",
  "iss": "http://localhost:8443/oauth/v2/oauth-anonymous",
  "sub": "62c839b8214aa1fe8cbcd823948a4bc705fbbba69c7666e334ee5c7fb348b60a",
  "aud": "https://mcp.demo.example",
  "iat": 1771434054,
  "purpose": "access_token",
  "customer_id": "178",
  "region": "USA",
  "client_id": "console-client",
  "client_type": "ai-agent",
  "agent_id": "autonomous-agent"
}
```

In this initial business flow, the Portfolio MCP Server implements the detailed business authorization.  
To do so it uses the following custom scopes and claims, and filters data by region and customer.  

- The `stocks/read` scope restricts the Azure LLM to read-only stock information.
- The `region` originates from an Entra ID attribute and restricts stocks to those allowed for the user's region.
- The `customer_id` originates from an Entra ID attribute for a custom user identity, that the MCP server needs.
- The `client_type` easily enables APIs to adjust authorization when an AI agent is present.

The Portfolio MCP Server returns authorized user-specific data to the agent and hence to the Azure LLM.  
The Azure LLM is able to operate on raw data in highly flexible ways, to provide business value.

## Embedded Access Tokens

If you use an external identity system like Entra ID, the Curity Identity Server receives a set of external tokens.  
The Curity Identity Server's access token can include an external token as a custom claim.  
For example, the autonomous agent could use the Entra ID access token to interact with Azure MCP servers.  

```json
{
  "idp_access_token": "eyJ0eXAiOiJKV1QiLCJub2 ..."
}
```

## AI Token Auditing

The autonomous agent routes all backend AI agent requests for secured resources through an internal gateway.  
The gateway can receive Curity tokens and write audit logs that include token attributes.  

The example deployment writes JSON audit logs that include business-centric claims and the agent identity.  
You can ship such logs to a log aggregation system to provide visibility of large scale agent access to secured resources.  

```json
{ 
    "log_type": "audit",
    "time": "2026-02-22T15:18:09Z",
    "target_host": "localhost",
    "target_path": "/mcp",
    "target_method": "POST",
    "client_id": "console-client",
    "agent_id": "autonomous-agent",
    "scope": "stocks/read",
    "audience": "https://mcp.demo.example",
    "delegation_id": "f8b69837-1d3e-4c8a-886f-82923c35955a",
    "customer_id": "178",
    "region": "USA"
}
```

An internal gateway can also serve as an initial point of authorization for all AI agent requests for secured resources.  
For example, the gateway could enforce rules like not allowing AI agents to send access tokens with particular scopes.  

# Autonomous Agent 

The autonomous agent is a backend agent and A2A server whose main role is to [integrate with the Azure LLM](src/AutonomousAgent.cs).  
The autonomous agent sends its opaque access token through an internal gateway to the LLM and MCP servers.  
In requests to MCP servers, the gateway performs token exchange to add agent identity attributes to access tokens.  

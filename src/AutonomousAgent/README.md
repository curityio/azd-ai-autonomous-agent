# Autonomous Agent 

The autonomous agent is an A2A server that uses Microsoft JWT bearer middleware to validate received access tokens.  
The agent then makes a token exchange request to get a new access token with which to call the Portfolio MCP Server.  
The main role of the autonomous agent is to [integrate with the Azure LLM](src/AutonomousAgent.cs) and the agent contains minimal logic.

## Portfolio MCP Server

The Portfolio MCP Server is an OAuth resource server that uses Microsoft JWT bearer middleware to validate access tokens.  
The Portfolio MCP Server is responsible for business authorization, using [custom token claims](src/StockToolsService.cs) to filter returned resources.  
The autonomous agent receives only authorized data and can safely manipulate that data.

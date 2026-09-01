#pragma warning disable OPENAI001
namespace IO.Curity.AutonomousAgent
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text.Json;
    using ModelContextProtocol.Client;
    using OpenAI.Responses;
    using IO.Curity.AutonomousAgent.Security;

    /*
     * Use the Microsoft MCP SDK to interact with the remote MCP server
     */
    public class McpToolsClient
    {
        private IEnumerable<McpClientTool> mcpTools;
        private readonly Configuration configuration;
        private readonly OAuthHttpClientHandler oauthHttpClientHandler;

        public McpToolsClient(Configuration configuration, OAuthHttpClientHandler oauthHttpClientHandler)
        {
            this.mcpTools = [];
            this.configuration = configuration;
            this.oauthHttpClientHandler = oauthHttpClientHandler;
        }

        /*
         * Translate MCP tools to functions for the OpenAI responses API
         */
        public async Task AddFunctionTools(IList<ResponseTool> toolList)
        {
            await this.GetMcpToolsAsync();
            foreach (var tool in this.mcpTools)
            {
                var functionTool = ResponseTool.CreateFunctionTool(
                    functionName: tool.Name,
                    functionDescription: tool.Description,
                    functionParameters:
                        BinaryData.FromString(
                            tool.JsonSchema.GetRawText()),
                    strictModeEnabled: false
                );
                toolList.Add(functionTool);
            }
        }

        /*
         * Call a particular MCP tool to get JSON data when the LLM requests it
         */
        public async Task<string> CallToolAsync(FunctionCallResponseItem call)
        {
            var tool = this.mcpTools.FirstOrDefault(mcp => mcp.Name == call.FunctionName);
            if (tool == null)
            {
                throw new InvalidOperationException($"The LLM requested an invalid tool name of ${call.FunctionName}");
            }

            var arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(call.FunctionArguments);
            if (arguments is null)
            {   
                arguments = new Dictionary<string, object?>();
            }

            var result = await tool.CallAsync(arguments);
            return JsonSerializer.Serialize(result);
        }

        /*
         * Get all tools from the remote MCP server
         */
        private async Task GetMcpToolsAsync()
        {
            var transportOptions = new HttpClientTransportOptions
            {
                Name = "Portfolio MCP Server",
                Endpoint = new Uri(configuration.PortfolioMcpServerUrl),
            };

            var httpClient = new HttpClient(oauthHttpClientHandler);
            var mcpClient = await McpClient.CreateAsync
            (
                new HttpClientTransport(transportOptions, httpClient)
            );

            this.mcpTools = await mcpClient.ListToolsAsync();
        }
    }   
}

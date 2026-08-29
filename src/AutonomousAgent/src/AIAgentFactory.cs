namespace IO.Curity.AutonomousAgent
{
    using System;
    using System.ClientModel.Primitives;
    using System.Net.Http;
    using Azure.AI.Projects;
    using Azure.Identity;
    using Microsoft.Agents.AI;
    using Microsoft.Extensions.AI;
    using ModelContextProtocol.Client;
    using IO.Curity.AutonomousAgent.Utilities;
    
    /*
     * The agent factory creates an autonomous agent as a thread safe singleton
     */
    public class AIAgentFactory
    {
        private readonly Configuration configuration;
        private readonly LlmHttpClientPolicy llmHttpClientPolicy;
        private readonly McpHttpClientHandler mcpHttpClientHandler;

        public AIAgentFactory(Configuration configuration, LlmHttpClientPolicy llmHttpClientPolicy, McpHttpClientHandler mcpHttpClientHandler)
        {
            this.configuration = configuration;
            this.llmHttpClientPolicy = llmHttpClientPolicy;
            this.mcpHttpClientHandler = mcpHttpClientHandler;
        }

        /*
         * Connect to the Azure model and create an agent, then register tools
         * Getting the Azure credential requires an AZURE_CLIENT_ID environment variable in deployed systems
         */
        public async Task<AIAgent> CreateAgentAsync()
        {
            var options = new AIProjectClientOptions();
            options.AddPolicy(this.llmHttpClientPolicy, PipelinePosition.PerCall);

            var aiProjectClient = new AIProjectClient(
                new Uri(this.configuration.AzureFoundryProjectUrl),
                new DefaultAzureCredential(),
                options);

            var tools = await this.GetMcpToolsAsync();

            return aiProjectClient.AsAIAgent(
                model: this.configuration.AzureAIModelName,
                name: "autonomous-agent",
                instructions: "You are a backend autonomous agent",
                tools: tools.ToArray()
            );
        }

        /*
         * Get MCP tools during the first user request, and supply an access token
         */
        private async Task<IEnumerable<AITool>> GetMcpToolsAsync()
        {
            var transportOptions = new HttpClientTransportOptions
            {
                Name = "Portfolio MCP Server",
                Endpoint = new Uri(this.configuration.PortfolioMcpServerUrl),
            };

            var httpClient = new HttpClient(this.mcpHttpClientHandler);
            var mcpClient = await McpClient.CreateAsync
            (
                new HttpClientTransport(transportOptions, httpClient)
            );

            IList<McpClientTool> mcpTools = await mcpClient.ListToolsAsync();
            return mcpTools.Cast<AITool>();
        }
    }
}

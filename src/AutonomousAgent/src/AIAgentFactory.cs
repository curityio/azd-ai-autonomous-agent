namespace IO.Curity.AutonomousAgent
{
    using System;
    using System.Net.Http;
    using Azure.AI.Projects;
    using Azure.Core;
    using Azure.Identity;
    using IO.Curity.AutonomousAgent.Security;
    using Microsoft.Agents.AI;
    using Microsoft.Extensions.AI;
    using ModelContextProtocol.Client;
    
    /*
     * The agent factory creates an autonomous agent as a thread safe singleton
     */
    public class AIAgentFactory
    {
        private readonly Configuration configuration;
        private readonly OAuthHttpClientHandler oauthHttpClientHandler;

        public AIAgentFactory(Configuration configuration, OAuthHttpClientHandler oauthHttpClientHandler)
        {
            this.configuration = configuration;
            this.oauthHttpClientHandler = oauthHttpClientHandler;
        }

        /*
         * Connect to the Azure model and create an agent, then register tools
         */
        public async Task<AIAgent> CreateAgentAsync()
        {
#pragma warning disable CA2252
            var aiProjectClient = new AIProjectClient(new Uri(this.configuration.AzureFoundryProjectUrl), this.GetManagedCredential());
#pragma warning restore CA2252

            var mcpTools = await this.GetMcpToolsAsync();
            return await aiProjectClient.CreateAIAgentAsync(
                name: "autonomous-agent",
                model: this.configuration.AzureAIModelName,
                instructions: "You are a backend autonomous agent",
                tools: mcpTools.ToArray()
            );
        }

        /*
         * Get an Azure managed credential with which to connect to the model
         */
        private TokenCredential GetManagedCredential()
        {
            if (this.configuration.IsLocalDevelopment)
            {
                return new AzureCliCredential();
            }
            else
            {
                return new ManagedIdentityCredential(configuration.ManagedIdentityClientId);
            }
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

            var httpClient = new HttpClient(this.oauthHttpClientHandler);
            var mcpClient = await McpClient.CreateAsync
            (
                new HttpClientTransport(transportOptions, httpClient)
            );

            IList<McpClientTool> mcpTools = await mcpClient.ListToolsAsync();
            return mcpTools.Cast<AITool>();
        }
    }
}

namespace IO.Curity.AutonomousAgent
{
    using System.Threading.Tasks;
    using A2A;
    using IO.Curity.AutonomousAgent.Security;
    using Microsoft.Agents.AI;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /*
     * The autonomous agent receives a natural language request from an external agent like Claude
     * The autonomous agent calls the LLM which can select outbound MCP or A2A requests that require security
     * - https://github.com/a2aproject/a2a-dotnet
     */
    public class AutonomousAgent : Controller
    {
        private readonly Configuration configuration;
        private readonly ILogger<AutonomousAgent> logger;
        private Lazy<Task<AIAgent>> agentFactory;

        /*
         * Create the agent in a thread safe manner on a background thread, during the first user request
         * The agent can then get tools from the MCP server with the user's access token
         */
        public AutonomousAgent(Configuration configuration, OAuthHttpClientHandler oauthHttpClientHandler, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.logger = new Logger<AutonomousAgent>(loggerFactory);

            this.agentFactory = new Lazy<Task<AIAgent>>(() => Task.Run(() =>
            {
                return new AIAgentFactory(this.configuration, oauthHttpClientHandler).CreateAgentAsync();
            }));
        }

        /*
         * Wire up A2A operations
         */
        public void Initialize(ITaskManager taskManager)
        {
            taskManager.OnAgentCardQuery = this.GetAgentCardAsync;
            taskManager.OnMessageReceived = this.ReceiveNaturalLanguageCommandAsync;
        }

        /*
         * Expose public agent card metadata 
         */
        [AllowAnonymous]
        [HttpGet(".well-known/agent-card.json")]
        public async Task<AgentCard> GetAgentCardWellKnownAsync()
        {
            var externalUrl = configuration.ExternalBaseUrl;
            return await this.GetAgentCardAsync(externalUrl, CancellationToken.None);
        }

        /*
         * Return an agent card to describe the A2A service
         */
        private async Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken)
        {
            var skill = new AgentSkill()
            {
                Id = "stocks",
                Name = "Stock portfolio operations",
                Description = "Manage stocks within a portfolio.",
                Tags = ["stocks", "portfolio"],
            };

            var scopes = new Dictionary<string, string>
            {
                [configuration.Scope] = "Read only access to a user portfolio",
            };

            var flows = new OAuthFlows()
            {
                AuthorizationCode = new AuthorizationCodeOAuthFlow
                (
                    new Uri(this.configuration.AuthorizationUrl),
                    new Uri(this.configuration.TokenUrl),
                    scopes
                ),
            };

            return new AgentCard
            {
                Name = "Autonomous Agent",
                Description = "Uses backend security to process natural language commands from an external agent",
                Url = agentUrl,
                Version = "1.0.0",
                DefaultInputModes = ["text"],
                DefaultOutputModes = ["text"],
                Skills = [skill],
                SecuritySchemes = new Dictionary<string, SecurityScheme>
                {
                    ["oauth2"] = new OAuth2SecurityScheme(flows),
                },
            };
        }

        /*
         * Process an A2A request and return an A2A response 
         */
        private async Task<A2AResponse> ReceiveNaturalLanguageCommandAsync(MessageSendParams messageSendParams, CancellationToken cancellationToken)
        {
            var command = messageSendParams.Message.Parts.OfType<TextPart>().First().Text;

            this.logger.LogDebug($">>> LLM request: {command}");
            var agent = await this.agentFactory.Value;
            var response = await agent.RunAsync(command);
            this.logger.LogDebug($">>> LLM response: {response.Text}");

            var message = new AgentMessage()
            {
                Role = MessageRole.Agent,
                MessageId = Guid.NewGuid().ToString(),
                ContextId = messageSendParams.Message.ContextId,
                Parts = [new TextPart { Text = response.Text }]
            };

            return message;
        }
    }
}

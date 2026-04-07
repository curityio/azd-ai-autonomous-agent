namespace IO.Curity.AutonomousAgent
{
    using System.Threading.Tasks;
    using A2A;
    using IO.Curity.AutonomousAgent.Security;
    using Microsoft.Agents.AI;
    using Microsoft.Extensions.Logging;

    /*
     * The autonomous agent receives a natural language request from an external agent like Claude
     * The autonomous agent calls the LLM which can select outbound MCP or A2A requests that require security
     * - https://github.com/a2aproject/a2a-dotnet
     */
    public class AutonomousAgent : IAgentHandler
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
         * Return the agent card information to A2A clients
         */
        public static AgentCard GetAgentCard(Configuration configuration) {

            //".well-known/agent-card.json", 
            var skill = new A2A.AgentSkill
            {
                Id = "stocks",
                Name = "Stock portfolio operations",
                Description = "Manage stocks within a portfolio.",
                Tags = ["stocks", "portfolio"],
            };

            var oauth2Scheme = new SecurityScheme
            {
                OAuth2SecurityScheme = new OAuth2SecurityScheme
                {
                    Flows = new OAuthFlows
                    {
                        AuthorizationCode = new AuthorizationCodeOAuthFlow
                        {
                            AuthorizationUrl = configuration.AuthorizationUrl,
                            TokenUrl = configuration.TokenUrl,
                            Scopes = new Dictionary<string, string>
                            {
                                [configuration.Scope] = "Read only access to a user portfolio",
                            },
                        }
                    }
                }
            };

            return new AgentCard
            {
                Name = "Autonomous Agent",
                Description = "Uses backend security to process natural language commands from an external agent",
                Version = "1.0.0",
                DefaultInputModes = ["text"],
                DefaultOutputModes = ["text"],
                Skills = [skill],
                SecuritySchemes = new Dictionary<string, SecurityScheme>
                {
                    ["oauth2"] = oauth2Scheme,
                },
            };
        }

        /*
         * Process an A2A request and return an A2A response 
         */
        public async Task ExecuteAsync(RequestContext context, AgentEventQueue eventQueue, CancellationToken cancellationToken)
        {
            var command = context.UserText ?? string.Empty;
            this.logger.LogDebug($">>> LLM request: {command}");

            var agent = await this.agentFactory.Value;
            var response = await agent.RunAsync(command);
            this.logger.LogDebug($">>> LLM response: {response.Text}");

            var responder = new MessageResponder(eventQueue, context.ContextId);
            await responder.ReplyAsync($"Echo: {response.Text}", cancellationToken);
        }
    }
}

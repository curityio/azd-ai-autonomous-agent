#pragma warning disable OPENAI001
namespace IO.Curity.AutonomousAgent
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using A2A;
    using Azure.AI.Extensions.OpenAI;
    using Azure.AI.Projects;
    using Azure.Identity;
    using Microsoft.Extensions.Logging;
    using OpenAI.Responses;
    using IO.Curity.AutonomousAgent.Security;

    /*
     * The autonomous agent receives a natural language request from an external app or agent
     * The autonomous agent calls the LLM which can trigger secured MCP requests
     * - https://github.com/a2aproject/a2a-dotnet
     */
    public class AutonomousAgent : IAgentHandler
    {
        private readonly Configuration configuration;
        private readonly OAuthHttpClientHandler oauthHttpClientHandler;
        private readonly ILogger<AutonomousAgent> logger;

        private readonly ProjectResponsesClient responsesClient;

        /*
         * Create the agent in a thread safe manner on a background thread, during the first user request
         * The agent can then get tools from the MCP server with the user's access token
         */
        public AutonomousAgent(
            Configuration configuration,
            OAuthHttpClientHandler oauthHttpClientHandler,
            ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.oauthHttpClientHandler = oauthHttpClientHandler;
            this.logger = new Logger<AutonomousAgent>(loggerFactory);
            
            var projectClient = new AIProjectClient(
                new Uri(this.configuration.AzureFoundryProjectUrl),
                new DefaultAzureCredential());

            this.responsesClient = projectClient
                .GetProjectOpenAIClient()
                .GetProjectResponsesClientForModel(this.configuration.AzureAIModelDeploymentName);
        }

        /*
         * Return the agent card information to A2A clients
         */
        public static AgentCard GetAgentCard(Configuration configuration) {

            var skill = new AgentSkill
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
                                [configuration.RequiredScope] = "Read only access to a user portfolio",
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
         * Receive an A2A request, call Foundry, then return an A2A response
         */
        public async Task ExecuteAsync(RequestContext context, AgentEventQueue eventQueue, CancellationToken cancellationToken)
        {
            var userCommand = context.UserText ?? string.Empty;
            this.logger.LogDebug($">>> LLM request: {userCommand}");

            try
            {
                var responseText = await this.CallFoundryModel(userCommand);
                this.logger.LogDebug($">>> LLM response received");

                var responder = new MessageResponder(eventQueue, context.ContextId);
                await responder.ReplyAsync(responseText, cancellationToken);
            }
            catch (Exception e)
            {
                this.logger.LogDebug($">>> LLM error response: {e.Message}");

                var responder = new MessageResponder(eventQueue, context.ContextId);
                await responder.ReplyAsync("Server problem encountered", cancellationToken);
            }
        }

        /*
         * Call Foundry using the OpenAI responses API and handle any LLM responses that trigger MCP tools
         */
        private async Task<string> CallFoundryModel(string userCommand)
        {
            var options = new CreateResponseOptions
            {
                InputItems =
                {
                    ResponseItem.CreateUserMessageItem(userCommand)
                },
                ReasoningOptions = new ResponseReasoningOptions
                {
                    ReasoningEffortLevel = ResponseReasoningEffortLevel.None
                }
            };

            var mcpToolsClient = new McpToolsClient(this.configuration, this.oauthHttpClientHandler);
            await mcpToolsClient.AddFunctionTools(options.Tools);

            while (true)
            {
                var finalText = new StringBuilder();
                var functionCalls = new List<FunctionCallResponseItem>();
            
                var response = await this.responsesClient.CreateResponseAsync(options);
                foreach (var output in response.Value.OutputItems)
                {
                    switch (output)
                    {
                        case FunctionCallResponseItem call:
                            this.logger.LogDebug($">>> LLM triggered MCP request: {call.CallId}, {call.FunctionName}");
                            functionCalls.Add(call);
                            break;

                        case MessageResponseItem message:
                            this.logger.LogDebug(">>> LLM received final response");
                            foreach (var content in message.Content)
                            {
                                finalText.Append(content.Text);
                            }
                            break;

                        default:
                            this.logger.LogDebug($">>> LLM unhandled response: {output.GetType().Name}");
                            break;
                    }

                    options.InputItems.Add(output);
                }

                foreach (var call in functionCalls)
                {
                    var jsonData = await mcpToolsClient.CallToolAsync(call);
                    var jsonResponseItem = new FunctionCallOutputResponseItem(call.CallId, jsonData);
                    options.InputItems.Add(jsonResponseItem);
                }

                if (functionCalls.Count == 0)
                {
                    return finalText.ToString();
                }
            }
        }
    }
}

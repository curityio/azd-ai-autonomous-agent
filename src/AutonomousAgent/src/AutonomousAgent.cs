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

            var scopes = new Dictionary<string, string>
            {
                [configuration.RequiredScope] = "Read only access to stocks",
            };

            var oauth2Scheme = new SecurityScheme
            {
                OAuth2SecurityScheme = new OAuth2SecurityScheme
                {
                    Flows = new OAuthFlows
                    {
                        ClientCredentials = new ClientCredentialsOAuthFlow
                        {
                            TokenUrl = configuration.TokenUrl,
                            Scopes = scopes,
                        },
                        AuthorizationCode = new AuthorizationCodeOAuthFlow
                        {
                            AuthorizationUrl = configuration.AuthorizationUrl,
                            TokenUrl = configuration.TokenUrl,
                            Scopes = scopes,
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

            var responder = new MessageResponder(eventQueue, context.ContextId);
            Func<string, Task> onChunk  = async chunk => await responder.ReplyAsync(chunk, cancellationToken);

            try
            {
                await this.CallFoundryModelAsync(userCommand, onChunk, cancellationToken);
            }
            catch (Exception e)
            {
                this.logger.LogDebug($">>> LLM error response: {e.Message}");
                await onChunk("Server problem encountered");
            }
        }

        /*
         * Call Foundry using the OpenAI responses API and handle any LLM responses that trigger MCP tools
         */
        private async Task CallFoundryModelAsync(string userCommand, Func<string, Task> onChunk, CancellationToken cancellationToken)
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
                },
                StreamingEnabled = true
            };

            var mcpToolsClient = new McpToolsClient(this.configuration, this.oauthHttpClientHandler);
            await mcpToolsClient.AddFunctionTools(options.Tools);

            for (var round = 0; round < 5; round++)
            {
                var functionCalls = new List<FunctionCallResponseItem>();

                await foreach (var update in this.responsesClient.CreateResponseStreamingAsync(options, cancellationToken))
                {
                    switch (update)
                    {
                        case StreamingResponseOutputTextDeltaUpdate textUpdate:
                            
                            if (!string.IsNullOrEmpty(textUpdate.Delta))
                            {
                                this.logger.LogDebug(">>> LLM response chunk received");
                                await onChunk(textUpdate.Delta);
                            }
                            break;
                    
                        case StreamingResponseOutputItemDoneUpdate doneUpdate:

                            options.InputItems.Add(doneUpdate.Item);
                            if (doneUpdate.Item is FunctionCallResponseItem call)
                            {
                                this.logger.LogDebug($">>> LLM triggered MCP request: {call.CallId}, {call.FunctionName}");
                                functionCalls.Add(call);
                            }
                            break;
                    }
                }

                if (functionCalls.Count == 0)
                {
                    return;
                }

                var mcpToolTasks = functionCalls.Select(async call =>
                {
                    var jsonData = await mcpToolsClient.CallToolAsync(call);
                    return new FunctionCallOutputResponseItem(call.CallId, jsonData);
                });

                var mcpToolResults = await Task.WhenAll(mcpToolTasks);
                foreach (var result in mcpToolResults)
                {
                    options.InputItems.Add(result);
                }
            }
        }
    }
}

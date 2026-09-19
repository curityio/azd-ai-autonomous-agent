namespace IO.Curity.ConsoleClient
{
    using System;
    using System.Net;
    using System.Text.Json.Nodes;
    using A2A;
    using IO.Curity.ConsoleClient.Security;

    /*
     * The agent client uses the A2A protocol to send natural language commands to the autonomous agent
     */
    public class AgentClient: HttpClientHandler
    {
        private readonly OAuthClient oauthClient;
        private readonly A2AClient a2aClient;

        /*
         * The agent makes requests to the agent and sends OAuth tokens
         */
        public AgentClient(Uri agentUrl, OAuthClient oauthClient)
        {
            this.a2aClient = new A2AClient(agentUrl, new HttpClient(this));
            this.oauthClient = oauthClient;
        }

        /*
         * Get the access token from the OAuth client and send it in the A2A request
         */
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Authorization", $"Bearer {oauthClient.GetAccessToken()}");
            
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var json = await response.Content.ReadAsStringAsync();
                var errorData = JsonNode.Parse(json);

                var error = errorData?["error"]?.GetValue<string>() ?? "http_error";
                var errorDescription = errorData?["error_description"]?.GetValue<string>() ?? "Problem encountered in an HTTP request";

                throw new ClientError(error, errorDescription)
                {
                    StatusCode = (int)response.StatusCode
                };
            };

            return response;
        }

        /*
         * Send a command to the agent, and use long running tasks when required
         */
        public async Task SendNaturalLanguageCommandAsync(string command, Action<string> onMessage)
        {
            var request = new SendMessageRequest
            {
                Message = new Message
                {
                    Role = Role.User,
                    Parts = [Part.FromText(command)]
                }
            };
            
            try
            {
                await foreach (var response in this.a2aClient.SendStreamingMessageAsync(request))
                {
                    if (response.PayloadCase == StreamResponseCase.Message)
                    {
                        var text = response.Message?.Parts?[0]?.Text;
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            var data = JsonNode.Parse(text);
                            var message = data?["message"]?.GetValue<string>() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                var type = data?["type"]?.GetValue<string>() ?? string.Empty;
                                if (type == "message")
                                {
                                    onMessage(message);
                                }

                                if (type == "error")
                                {
                                    var statusCode = data?["statusCode"]?.GetValue<int>() ?? 0;
                                    var error = data?["error"]?.GetValue<string>() ?? string.Empty;
                                    var errorDescription = data?["error_description"]?.GetValue<string>() ?? string.Empty;
                                    onMessage($"Agent problem encountered: {statusCode}, {error}: {errorDescription}");
                                }
                            }
                        }
                    }
                }
            }
            catch (A2AException ex)
            {
                throw new ClientError(ex.ErrorCode.ToString(), ex.Message);
            }
        }
    }
}

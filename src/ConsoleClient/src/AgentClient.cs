namespace IO.Curity.ConsoleClient
{
    using System;
    using System.Net;
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
            return await base.SendAsync(request, cancellationToken);
        }

        /*
         * Send a command to the agent, and use long running tasks when required
         */
        public async Task<string> SendNaturalLanguageCommandAsync(string command)
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
                var response = await this.a2aClient.SendMessageAsync(request);
                return response?.Message?.Parts?[0]?.Text ?? string.Empty;
            }
            catch (A2AException e)
            {
                throw new ClientError(e.ErrorCode.ToString(), e.Message);
            }
            catch (HttpRequestException e)
            {   
                if (e.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new ClientError("invalid_token", "Missing, invalid or expired access token")
                    {
                        StatusCode = 401
                    };
                }

                var error = new ClientError("connection_error", e.Message);
                if (e.StatusCode != null)
                {
                    error.StatusCode = (int)e.StatusCode;
                }
                throw error;
            }
        }
    }
}

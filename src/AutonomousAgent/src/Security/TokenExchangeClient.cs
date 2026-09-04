namespace IO.Curity.AutonomousAgent.Security
{
    using System.Net;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using Microsoft.Extensions.Logging;
    using IO.Curity.AutonomousAgent.Utilities;

    /*
     * Implement token exchange to enable the agent to call an upstream MCP server
     */
    public sealed class TokenExchangeClient
    {
        private readonly Configuration configuration;
        private readonly TokenCache cache;
        private readonly ILogger<TokenExchangeClient> logger;

        public TokenExchangeClient(Configuration configuration, TokenCache cache, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.cache = cache;
            this.logger = new Logger<TokenExchangeClient>(loggerFactory);
        }

        /*
         * In this deployment, MCP calls use token exchange and add an mcp scope to get agent attributes into an access token
         * The token exchange also sets the audience that the target MCP server requires
         */
        public async Task<string> ExchangeAccessToken(string receivedAccessToken)
        {
            var cachedToken = await this.cache.GetItemAsync(receivedAccessToken);
            if (!string.IsNullOrWhiteSpace(cachedToken))
            {
                return cachedToken;
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("accept", "application/json");
                var requestData = new[]
                {
                    new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:token-exchange"),
                    new KeyValuePair<string, string>("client_id", this.configuration.TokenExchangeClientId),
                    new KeyValuePair<string, string>("client_secret", this.configuration.TokenExchangeClientSecret),
                    new KeyValuePair<string, string>("subject_token", receivedAccessToken),
                    new KeyValuePair<string, string>("subject_token_type", "urn:ietf:params:oauth:token-type:access_token"),
                    new KeyValuePair<string, string>("audience", this.configuration.TokenExchangeTargetAudience),
                };

                HttpResponseMessage response;
                try
                {
                    response = await client.PostAsync(this.configuration.TokenUrl, new FormUrlEncodedContent(requestData));
                }
                catch (Exception ex)
                {
                    this.logger.LogError($">>> Token exchange request error: {ex.Message}");
                    throw ErrorFactory.CreateServerError();
                }

                if (!response.IsSuccessStatusCode)
                {
                    await this.LogRemoteError(response);

                    if(response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw ErrorFactory.CreateUnauthorizedError();
                    }
                    else
                    {
                        throw ErrorFactory.CreateServerError();
                    }
                }

                var responseText = await response.Content.ReadAsStringAsync();
                var responseData = JsonNode.Parse(responseText);
                var exchangedAccessToken = responseData?["access_token"]?.GetValue<string>();
                if (string.IsNullOrWhiteSpace(exchangedAccessToken))
                {
                    this.logger.LogError(">>> No access token was received in a token exchange response");
                    throw ErrorFactory.CreateServerError();
                }
                
                await this.cache.SetItemAsync(receivedAccessToken, exchangedAccessToken);
                Console.WriteLine(receivedAccessToken);
                Console.WriteLine(exchangedAccessToken);

                return exchangedAccessToken;
            }
        }

        /*
         * Log details from the external system
         */
        private async Task LogRemoteError(HttpResponseMessage response)
        {
            var error = string.Empty;
            var errorDescription = string.Empty;
            
            try
            {
                var responseText = await response.Content.ReadAsStringAsync();
                var responseData = JsonNode.Parse(responseText);
                error = responseData?["error"]?.GetValue<string>();
                errorDescription = responseData?["error_description"]?.GetValue<string>();
            }
            catch (JsonException)
            {
            }

            this.logger.LogError($">>> Token exchange response error: {response.StatusCode}, {error}, {errorDescription} ");
        }
    }
}

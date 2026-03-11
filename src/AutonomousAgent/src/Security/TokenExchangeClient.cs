namespace IO.Curity.AutonomousAgent.Security
{
    using System.Text.Json.Nodes;
    using Microsoft.Extensions.Logging;

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
         * Outbound MCP or A2A calls can use the incoming access token, an embedded access token or token exchange
         */
        public async Task<string?> ExchangeAccessToken(string receivedAccessToken)
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

                try
                {
                    var response = await client.PostAsync(this.configuration.TokenUrl, new FormUrlEncodedContent(requestData));
                    var responseText = await response.Content.ReadAsStringAsync();
                    var responseData = JsonNode.Parse(responseText);

                    if (!response.IsSuccessStatusCode)
                    {
                        var tokenErrorCode = responseData?["error"]?.GetValue<string>() ??
                            "token_exchange_error";
                        var tokenErrorDescription = responseData?["error_description"]?.GetValue<string>() ??
                            "Problem encountered exchanging the access token";

                        this.logger.LogError($">>> Token exchange error: {tokenErrorCode} {tokenErrorDescription} ");
                        return null;
                    }

                    var exchangedAccessToken = responseData?["access_token"]?.GetValue<string>();
                    if (string.IsNullOrWhiteSpace(exchangedAccessToken))
                    {
                        this.logger.LogError(">>> No access token was received in a token exchange response");
                        return null;
                    }
                    
                    await this.cache.SetItemAsync(receivedAccessToken, exchangedAccessToken);
                    return exchangedAccessToken;
                }
                catch (HttpRequestException exception)
                {
                    this.logger.LogError($">>> Unable to connect to the token endpoint: {exception.Message}");
                    return null;
                }
            }
        }
    }
}

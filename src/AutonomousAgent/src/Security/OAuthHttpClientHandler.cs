namespace IO.Curity.AutonomousAgent.Security
{
    using System.Net.Http;
    using Microsoft.Extensions.Logging;

    /*
     * An HTTP handler to add OAuth access tokens to outbound MCP client or A2A requests
     */
    public sealed class OAuthHttpClientHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<OAuthHttpClientHandler> logger;
        private readonly TokenExchangeClient tokenExchangeClient;

        public OAuthHttpClientHandler(IHttpContextAccessor httpContextAccessor, TokenExchangeClient tokenExchangeClient, ILoggerFactory loggerFactory)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.tokenExchangeClient = tokenExchangeClient;
            this.logger = new Logger<OAuthHttpClientHandler>(loggerFactory);
            this.InnerHandler = new HttpClientHandler();
        }

        /*
         * Outbound MCP or A2A calls can use the incoming access token, an embedded access token or token exchange
         */
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var receivedAccessToken = this.GetAccessToken();
            if (!string.IsNullOrWhiteSpace(receivedAccessToken))
            {
                var exchangedAccessToken = await this.tokenExchangeClient.ExchangeAccessToken(receivedAccessToken);
                if (!string.IsNullOrWhiteSpace(exchangedAccessToken))
                {
                    this.logger.LogDebug($">>> Agent remote request: {request.Method} {request.RequestUri} ");
                    request.Headers.Add("Authorization", $"Bearer {exchangedAccessToken}");
                    var response = await base.SendAsync(request, cancellationToken);
                    this.logger.LogDebug($">>> Agent remote response status: {response.StatusCode}");
                    return response;
                }
            }
            
            logger.LogError($"Unable to get an access token with which to call the MCP server");
            throw new InvalidOperationException($"Agent problem encountered during data access");
        }

        /*
         * Get the received access token from the external client that sent a secured A2A request
         */
        private string GetAccessToken()
        {
            var authorization = this.httpContextAccessor.HttpContext?.Request.GetHeader("authorization");
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var parts = authorization.Split(' ');
                if (parts.Length == 2 && parts[0].ToLowerInvariant() == "bearer")
                {
                   return parts[1];
                }
            }

            return string.Empty;
        }
    }
}

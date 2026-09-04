namespace IO.Curity.AutonomousAgent.Security
{
    using System.Net;
    using System.Net.Http;
    using System.Text.Json.Nodes;
    using Microsoft.Extensions.Logging;
    using IO.Curity.AutonomousAgent.Utilities;

    /*
     * An HTTP handler to add OAuth access tokens to outbound MCP tool requests
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
         * First do token exchange to get agent attributes into the access token
         * Then call MCP tools with the new access token
         */
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var receivedAccessToken = this.GetAccessToken();
            if (string.IsNullOrWhiteSpace(receivedAccessToken))
            {
                throw ErrorFactory.CreateUnauthorizedError();
            }

            var exchangedAccessToken = await this.tokenExchangeClient.ExchangeAccessToken(receivedAccessToken);

            HttpResponseMessage response;
            try
            {
                request.Headers.Add("Authorization", $"Bearer {exchangedAccessToken}");
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogDebug($">>> MCP tool request error: {ex.Message}");
                throw ErrorFactory.CreateServerError();
            }

            if (!response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();
                var responseData = JsonNode.Parse(responseText);
                
                this.LogRemoteError(response.StatusCode, responseData);
                if(response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw ErrorFactory.CreateUnauthorizedError();
                }
                else
                {
                    throw ErrorFactory.CreateServerError();
                }
            }

            return response;
        }

        /*
         * Get the access token from the external client that sent an A2A request
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

        /*
         * Log details from the external system
         */
        private void LogRemoteError(HttpStatusCode statusCode, JsonNode? responseData)
        {
            var error = responseData?["error"]?.GetValue<string>() ??
                "token_exchange_error";
            var errorDescription = responseData?["error_description"]?.GetValue<string>() ??
                "Problem encountered calling an MCP tool";
            this.logger.LogError($">>> MCP tool response error: {statusCode}, {error}, {errorDescription} ");
        }
    }
}

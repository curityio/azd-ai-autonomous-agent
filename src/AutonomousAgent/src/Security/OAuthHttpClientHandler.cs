namespace IO.Curity.AutonomousAgent.Security
{
    using System.Net;
    using System.Net.Http;
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
         * Outbound calls use token exchange and send an access token with agent attributes to the MCP server
         */
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var receivedAccessToken = this.GetAccessToken();
            if (string.IsNullOrWhiteSpace(receivedAccessToken))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            try {

                var (statusCode, exchangedAccessToken) = await this.tokenExchangeClient.ExchangeAccessToken(receivedAccessToken);
                if (statusCode != HttpStatusCode.OK)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }

                request.Headers.Add("Authorization", $"Bearer {exchangedAccessToken}");
                var response = await base.SendAsync(request, cancellationToken);
                this.logger.LogDebug($">>> Agent remote response status: {response.StatusCode}");
                return response;
            }
            catch (UnauthorizedAccessException)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
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
    }
}

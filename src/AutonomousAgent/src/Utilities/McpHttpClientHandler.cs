namespace IO.Curity.AutonomousAgent.Utilities
{
    using System.Net.Http;
    using Microsoft.Extensions.Logging;

    /*
     * An HTTP handler to add credentials to outgoing MCP requests
     */
    public sealed class McpHttpClientHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<McpHttpClientHandler> logger;

        public McpHttpClientHandler(Configuration configuration, IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.logger = new Logger<McpHttpClientHandler>(loggerFactory);
            this.InnerHandler = new HttpClientHandler();
        }

        /*
         * Outbound MCP, A2A or LLM calls can send the access token
         */
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var receivedAccessToken = this.httpContextAccessor.GetAccessToken();
            if (string.IsNullOrWhiteSpace(receivedAccessToken))
            {
                logger.LogError($"The incoming access token did not contain an access token with which to call the MCP server");
                throw new UnauthorizedAccessException();
            }

            this.logger.LogDebug($">>> Agent remote request: {request.Method} {request.RequestUri} ");
            request.Headers.Add("Authorization", $"Bearer {receivedAccessToken}");
            var response = await base.SendAsync(request, cancellationToken);
            this.logger.LogDebug($">>> Agent remote response status: {response.StatusCode}");
            return response;
        }
    }
}

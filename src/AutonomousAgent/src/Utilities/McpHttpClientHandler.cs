namespace IO.Curity.AutonomousAgent.Utilities
{
    using System.Net.Http;
    using Azure.Core;
    using Azure.Identity;
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
         * Outbound MCP, A2A or LLM calls can send the access token and a workload identity
         */
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var receivedAccessToken = this.httpContextAccessor.GetAccessToken();
            if (string.IsNullOrWhiteSpace(receivedAccessToken))
            {
                logger.LogError($"The incoming access token did not contain an access token with which to call the MCP server");
                throw new UnauthorizedAccessException();
            }

            var workloadCredential = await this.GetWorkloadCredential();

            this.logger.LogDebug($">>> Agent remote request: {request.Method} {request.RequestUri} ");
            request.Headers.Add("Authorization", $"Bearer {receivedAccessToken}");
            request.Headers.Add("Workload-Identity", workloadCredential);
            var response = await base.SendAsync(request, cancellationToken);
            this.logger.LogDebug($">>> Agent remote response status: {response.StatusCode}");
            return response;
        }

        /*
         * A backend agent should use a workload credential to strongly authenticate with other internal components
         * This example uses an Azure access token but that is not a true workload identity
         * A more complete Azure deployment might send a Kubernetes service account token
         * - https://curity.io/resources/learn/oauth-client-credentials-kubernetes/
         */
        private async Task<string> GetWorkloadCredential()
        {
            var workloadCredential = new DefaultAzureCredential();
            var workloadIdentityToken = await workloadCredential.GetTokenAsync(new TokenRequestContext(new[]
                {
                    "https://management.azure.com/.default"
                })
            );

            return workloadIdentityToken.Token;
        }
    }
}

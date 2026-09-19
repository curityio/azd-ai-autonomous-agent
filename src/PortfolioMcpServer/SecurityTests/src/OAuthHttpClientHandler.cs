namespace IO.Curity.PortfolioMcpServer.SecurityTests
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Http;
    using System.Text.Json.Nodes;
    using IO.Curity.PortfolioMcpServer.SecurityTests.Utilities;

    /*
     * An HTTP handler to send the access token for the current test in MCP client requests
     */
    public sealed class OAuthHttpClientHandler : DelegatingHandler
    {
        private readonly string accessToken;

        public OAuthHttpClientHandler(string accessToken)
        {
            this.accessToken = accessToken;
            this.InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Authorization", $"Bearer {this.accessToken}");
            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();
                var responseData = JsonNode.Parse(responseText);
                var error = responseData?["error"]?.GetValue<string>() ?? string.Empty;
                var errorDescription = responseData?["error_description"]?.GetValue<string>() ?? string.Empty;
                throw new McpError(response.StatusCode, error, errorDescription);
            }

            return response;
        }
    }
}

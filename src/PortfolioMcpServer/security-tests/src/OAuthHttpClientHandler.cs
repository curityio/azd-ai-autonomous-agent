namespace IO.Curity.PortfolioMcpServer.SecurityTests
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Http;

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
            return await base.SendAsync(request, cancellationToken);
        }
    }
}

namespace IO.Curity.PortfolioMcpServer.SecurityTests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using ModelContextProtocol.Client;
    using ModelContextProtocol.Protocol;
    using Xunit;
    using IO.Curity.PortfolioMcpServer.Entities;

    /*
     * Demonstrates how developers can test MCP client to MCP server security
     * This avoids the need for MCP server developers to run complex setups locally
     */
    public class SecurityTests : IClassFixture<SecurityTestFixture>
    {
        private SecurityTestFixture data;
        private readonly ITestOutputHelper output;

        public SecurityTests(SecurityTestFixture data, ITestOutputHelper output)
        {
            this.data = data;
            this.output = output;
        }

        /*
         * Get current stock prices with a valid access token
         */
        [Fact]
        [Trait("Category", "Security")]
        public async Task SecurityTests_GetCurrentStockPrices_SucceedsWithValidAccessToken()
        {
            var options = new MockTokenOptions(this.data.Configuration)
            {
                CustomerId = "195",
                Region = "Europe",
                AgentClaims = new Utilities.AgentClaims()
                {
                    AgentId = "example-agent",
                    AgentRole = "analyst",
                    AgentDepartment = "finance"
                }
            };

            Func<McpClient, Task> mcpToolAction = async (mcpClient) =>
            {
                var result = await mcpClient.CallToolAsync("get_current_stock_prices");
                var stocks = this.DeserializeReponse<Stock[]>(result);
                Assert.Equal(2, stocks?.Length);
            };

            await this.RunTest(options, mcpToolAction);
        }

        /*
         * An access token with an invalid audience is rejected with a 401 error
         */
        [Fact]
        [Trait("Category", "Security")]
        public async Task SecurityTests_ListTools_Returns401ForAccessTokenWithInvalidAudience()
        {
            var options = new MockTokenOptions(this.data.Configuration)
            {
                CustomerId = "898",
                Region = "Europe",
                Audience = "https://untrusted.audience",
            };

            Func<McpClient, Task> mcpToolAction = async (mcpClient) =>
            {
                await mcpClient.ListToolsAsync(cancellationToken: CancellationToken.None);
            };

            Action<HttpRequestException> errorAction = async (ex) =>
            {
                Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
            };

            await this.RunTest(options, mcpToolAction, errorAction);
        }

        /*
         * A test template to reduce repeated code
         */
        private async Task RunTest(MockTokenOptions options, Func<McpClient, Task> mcpToolAction, Action<HttpRequestException>? errorAction = null)
        {
            try
            {   
                var accessToken = this.data.AccessTokenIssuer.IssueAccessToken(options);
                var mcpClient = await this.CreateMcpClient(accessToken);
                try
                {
                    await mcpToolAction(mcpClient);
                    if (errorAction != null)
                    {
                        Assert.Fail("The tool request succeeded expectedly");
                    }
                }
                finally
                {
                    await mcpClient.DisposeAsync();
                }
            }
            catch (HttpRequestException ex)
            {
                if (errorAction == null)
                {
                    Assert.Fail("The tool request failed unexpectedly");
                }
                errorAction(ex);
            }
        }

        /*
         * Create the MCP client for a test
         */
        private Task<McpClient> CreateMcpClient(string accessToken)
        {
            var transportOptions = new HttpClientTransportOptions
            {
                Name = "Portfolio MCP Server",
                Endpoint = new Uri(this.data.Configuration.PortfolioMcpServerUrl),
            };

            var httpClient = new HttpClient(new OAuthHttpClientHandler(accessToken));
            return McpClient.CreateAsync
            (
                new HttpClientTransport(transportOptions, httpClient)
            );
        }

        /*
         * Deserialize an MCP response
         */
        private T? DeserializeReponse<T>(CallToolResult result)
        {
            var responseText = result?.Content?.OfType<TextContentBlock>().First().Text ?? string.Empty;
            return JsonSerializer.Deserialize<T>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
        }
    }
}

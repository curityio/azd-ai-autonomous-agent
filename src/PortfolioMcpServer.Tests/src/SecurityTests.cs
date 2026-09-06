namespace IO.Curity.PortfolioMcpServer.Tests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Jose;
    using ModelContextProtocol.Client;
    using ModelContextProtocol.Protocol;
    using Xunit;
    using IO.Curity.PortfolioMcpServer.Entities;
    using IO.Curity.PortfolioMcpServer.Tests.Utilities;

    /*
     * Example tests to demonstrate how developers can test MCP server security with access token attributes
     */
    [Trait("Category", "Security")]
    public class SecurityTests : IClassFixture<SecurityTestFixture>
    {
        private SecurityTestFixture data;

        public SecurityTests(SecurityTestFixture data)
        {
            this.data = data;
        }

        [Fact]
        public async Task ListTools_Succeds_WithValidAccessToken()
        {
            var options = new MockTokenOptions(this.data.Configuration);

            Func<McpClient, Task> mcpToolAction = async (mcpClient) =>
            {
                var results = await mcpClient.ListToolsAsync(cancellationToken: CancellationToken.None);
                Assert.True(results.Count == 2, "Unexpected tool count");
            };

            await this.RunTest(options, mcpToolAction);
        }

        [Fact]
        public async Task GetPortfolio_Succeeds_WithValidAccessToken()
        {
            var options = new MockTokenOptions(this.data.Configuration);

            Func<McpClient, Task> mcpToolAction = async (mcpClient) =>
            {
                var result = await mcpClient.CallToolAsync("get_portfolio");
                var portfolio = this.DeserializeReponse<Portfolio>(result);
                Assert.True(portfolio.Transactions.Length > 0, "Portfolio has no transactions");

                foreach(var transaction in portfolio.Transactions)
                {
                    Assert.True(transaction.CustomerId == options.CustomerId, "Customer could access another customer's transactions");
                }
            };

            await this.RunTest(options, mcpToolAction);
        }

        [Fact]
        public async Task GetStocks_Succeeds_WithValidAccessToken()
        {
            var options = new MockTokenOptions(this.data.Configuration);

            Func<McpClient, Task> mcpToolAction = async (mcpClient) =>
            {
                var result = await mcpClient.CallToolAsync("get_stocks");
                var stocks = this.DeserializeReponse<Stock[]>(result);
                Assert.True(stocks.Length > 0, "No stocks were found");

                foreach(var stock in stocks)
                {
                    Assert.True(stock.Region == options.Region, "Customer could access another region's stocks");
                }
            };

            await this.RunTest(options, mcpToolAction);
        }

        [Fact]
        public async Task ListTools_Returns401_ForAccessTokenWithInvalidAudience()
        {
            var options = new MockTokenOptions(this.data.Configuration)
            {
                Audience = "https://mcp.other.com"
            };

            Func<McpClient, Task> mcpToolAction = async (mcpClient) =>
            {
                await mcpClient.ListToolsAsync(cancellationToken: CancellationToken.None);
            };

            Action<McpError> errorAction = (error) =>
            {
                Assert.True(error.StatusCode == HttpStatusCode.Unauthorized, "Unexpected status code");
                Assert.True(error.Code == "invalid_token", "Unexpected error code");
            };

            await this.RunTest(options, mcpToolAction, errorAction);
        }

        [Fact]
        public async Task GetPortfolio_Returns401_ForAccessTokenWithInvalidIssuer()
        {
            var options = new MockTokenOptions(this.data.Configuration)
            {
                Issuer = "https://issuer.other.com"
            };

            Func<McpClient, Task> mcpToolAction = async (mcpClient) =>
            {
                await mcpClient.CallToolAsync("get_portfolio");
            };

            Action<McpError> errorAction = (error) =>
            {
                Assert.True(error.StatusCode == HttpStatusCode.Unauthorized, "Unexpected status code");
                Assert.True(error.Code == "invalid_token", "Unexpected error code");
            };

            await this.RunTest(options, mcpToolAction, errorAction);
        }

        [Fact]
        public async Task GetStocks_Returns401_ForAccessTokenWithInvalidSigningKey()
        {
            var keypair = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var maliciousKey = new Jwk(keypair, true);
            
            Jwk tokenSigningPublicKey = new Jwk(keypair, false)
            {
                Alg = "ES256",
                KeyId = Guid.NewGuid().ToString(),
            };

            var options = new MockTokenOptions(this.data.Configuration)
            {
                SigningKey = new Jwk(keypair, false)
                {
                    Alg = "ES256",
                    KeyId = Guid.NewGuid().ToString(),
                }
            };

            Func<McpClient, Task> mcpToolAction = async (mcpClient) =>
            {
                await mcpClient.CallToolAsync("get_portfolio");
            };

            Action<McpError> errorAction = (error) =>
            {
                Assert.True(error.StatusCode == HttpStatusCode.Unauthorized, "Unexpected status code");
                Assert.True(error.Code == "invalid_token", "Unexpected error code");
            };

            await this.RunTest(options, mcpToolAction, errorAction);
        }

        [Fact]
        public async Task GetPortfolio_Returns403_ForMissingRequiredScope()
        {
            var options = new MockTokenOptions(this.data.Configuration)
            {
                Scope = "openid profile"
            };

            Func<McpClient, Task> mcpToolAction = async (mcpClient) =>
            {
                await mcpClient.CallToolAsync("get_portfolio");
            };

            Action<McpError> errorAction = (error) =>
            {
                Assert.True(error.StatusCode == HttpStatusCode.Forbidden, "Unexpected status code");
                Assert.True(error.Code == "insufficient_scope", "Unexpected error code");
            };

            await this.RunTest(options, mcpToolAction, errorAction);
        }

        [Fact]
        public async Task GetPortfolio_Returns403_ForMissingAgentClaims()
        {
            var options = new MockTokenOptions(this.data.Configuration)
            {
                AgentClaims = null,
            };

            Func<McpClient, Task> mcpToolAction = async (mcpClient) =>
            {
                await mcpClient.CallToolAsync("get_portfolio");
            };

            Action<McpError> errorAction = (error) =>
            {
                Assert.True(error.StatusCode == HttpStatusCode.Forbidden, "Unexpected status code");
                Assert.True(error.Code == "insufficient_scope", "Unexpected error code");
            };

            await this.RunTest(options, mcpToolAction, errorAction);
        }

        [Fact]
        public async Task GetStocks_Returns403_ForInvalidAgentDepartment()
        {
            var options = new MockTokenOptions(this.data.Configuration)
            {
                AgentClaims = new AgentClaims
                {
                    AgentId = "example-agent",
                    AgentRole = "analyst",
                    AgentDepartment = "marketing",
                }
            };

            Func<McpClient, Task> mcpToolAction = async (mcpClient) =>
            {
                await mcpClient.CallToolAsync("get_portfolio");
            };

            Action<McpError> errorAction = (error) =>
            {
                Assert.True(error.StatusCode == HttpStatusCode.Forbidden);
                Assert.True(error.Code == "insufficient_scope");
            };

            await this.RunTest(options, mcpToolAction, errorAction);
        }

        /*
         * A test template to reduce repeated code
         */
        private async Task RunTest(MockTokenOptions options, Func<McpClient, Task> mcpToolAction, Action<McpError>? errorAction = null)
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
            catch (McpError error)
            {
                if (errorAction == null)
                {
                    Assert.Fail("The tool request failed unexpectedly");
                }
                errorAction(error);
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
        private T DeserializeReponse<T>(CallToolResult result)
        {
            var responseText = result?.Content?.OfType<TextContentBlock>().First().Text ?? string.Empty;
            T? deserialized = JsonSerializer.Deserialize<T>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return deserialized!;
            
        }
    }
}

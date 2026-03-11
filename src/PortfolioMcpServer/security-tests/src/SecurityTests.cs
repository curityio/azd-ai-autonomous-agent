namespace IO.Curity.PortfolioMcpServer.SecurityTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using ModelContextProtocol.Client;
    using ModelContextProtocol.Protocol;
    using Xunit;

    /*
     * Demonstrates how developers can test MCP client to MCP server security
     * This avoids the need for MCP server developers to run complex setups locally
     */
    public class SecurityTests : IClassFixture<SecurityTestFixture>
    {
        private SecurityTestFixture data;

        public SecurityTests(SecurityTestFixture data)
        {
            this.data = data;
        }

        /*
         * List tools fails when the access token has an invalid audience
         * The Portfolio MCP Server requires an audience of https://mcp.demo.example so does not accept the below access token
         */
        [Fact]
        [Trait("Category", "Security")]
        public async Task SecureMcpRequest_ListTools_Returns401ForAccessTokenWithInvalidAudience()
        {
            var options = new MockTokenOptions(this.data.Configuration)
            {
                CustomerId = "898",
                Region = "Europe",
                Audience = "https://agent.demo.example",
            };

            var accessToken = this.data.AuthorizationServer.IssueAccessToken(options);
            try
            {
                var mcpClient = await this.CreateMcpClient(accessToken);
                IList<McpClientTool> mcpTools = await mcpClient.ListToolsAsync(cancellationToken: CancellationToken.None);
                Assert.Fail("The tools request did not fail as expected");
                await mcpClient.DisposeAsync();
            }
            catch (HttpRequestException ex)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
            }
        }

        /*
         * Get available stocks with a valid access token
         */
        [Fact]
        [Trait("Category", "Security")]
        public async Task SecureMcpRequest_GetAvailableStocks_SucceedsWithValidAccessToken()
        {
            var options = new MockTokenOptions(this.data.Configuration)
            {
                CustomerId = "195",
                Region = "Europe"
            };

            var accessToken = this.data.AuthorizationServer.IssueAccessToken(options);
            
            var mcpClient = await this.CreateMcpClient(accessToken);
            var response = await mcpClient.CallToolAsync(toolName: "get_available_stocks", cancellationToken: CancellationToken.None);
            var responseText = response?.Content?.OfType<TextContentBlock>().First().Text ?? string.Empty;

            var stocks = JsonSerializer.Deserialize<Stock[]>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal(2, stocks?.Length);
            await mcpClient.DisposeAsync();
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
    }
}

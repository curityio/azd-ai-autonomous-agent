namespace IO.Curity.PortfolioMcpServer
{
    using System.ComponentModel;
    using System.Security.Claims;
    using ModelContextProtocol.Server;
    using IO.Curity.PortfolioMcpServer.Entities;

    /*
     * The MCP server authorizes access to stocks using customer_id and region claims in the user's access token
     */
    [McpServerToolType]
    public sealed class StocksToolsService
    {
        private readonly StocksRepository repository;
        private readonly ClaimsPrincipal claimsPrincipal;
        private readonly ILogger<StocksToolsService> logger;

        /*
         * Inject a data repository and the claims principal into the logic class
         */
        public StocksToolsService(StocksRepository repository, ClaimsPrincipal claimsPrincipal, ILogger<StocksToolsService> logger)
        {
            this.repository = repository;
            this.claimsPrincipal = claimsPrincipal;
            this.logger = logger;
        }

        /*
         * Use the customer ID and region from the access token to return restricted data to LLMs from the user's portfolio
         */
        [McpServerTool, Description("""
            Returns the customer's portfolio with its entire history of transactions.
        """)]
        public Portfolio GetPortfolio()
        {
            var customerId = this.GetClaim("customer_id");
            var region = this.GetClaim("region");
            this.logger.LogDebug($"Returning portfolio for customer {customerId} and region {region}");
            return this.repository.GetPortfolio(customerId, region);
        }

        [McpServerTool, Description("""
            Returns all stocks for the customer's region with current prices.
        """)]
        public Stock[] GetStocks()
        {
            var region = this.GetClaim("region");
            this.logger.LogDebug($"Returning stocks and current prices for region {region}");
            return this.repository.GetStocks(region);
        }

        private string GetClaim(string name)
        {
            var value = this.claimsPrincipal.FindFirst(c => c.Type == name)?.Value;
            return value ?? string.Empty;
        }
    }
}

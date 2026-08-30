namespace IO.Curity.PortfolioMcpServer
{
    using System.ComponentModel;
    using System.Security.Claims;
    using IO.Curity.PortfolioMcpServer.Entities;
    using ModelContextProtocol.Server;

    /*
     * The MCP server authorizes access to stocks using customer_id and region claims in the user's access token
     */
    [McpServerToolType]
    public sealed class StocksToolsService
    {
        private readonly DataRepository repository;
        private readonly ClaimsPrincipal claimsPrincipal;
        private readonly ILogger<StocksToolsService> logger;

        /*
         * Inject a data repository and the claims principal into the logic class
         */
        public StocksToolsService(DataRepository repository, ClaimsPrincipal claimsPrincipal, ILogger<StocksToolsService> logger)
        {
            this.repository = repository;
            this.claimsPrincipal = claimsPrincipal;
            this.logger = logger;
        }

        /*
         * Use custom attributes from the access token and audit identity attributes if required
         * This method restricts data returned to LLMs to the user's portfolio, identified by the customer ID and region
         */
        [McpServerTool, Description("""
            Returns the customer's portfolio with its entire history of transactions.
            The MCP client can add all transactions to get the current value of the portfolio.
        """)]
        public Portfolio GetPortfolio()
        {
            var customerId = this.GetClaim("customer_id");
            var region = this.GetClaim("region");
            this.logger.LogDebug($"Returning portfolio for customer {customerId} and region {region}");
            return this.repository.GetPortfolio(customerId, region);
        }

        private string GetClaim(string name)
        {
            var value = this.claimsPrincipal.FindFirst(c => c.Type == name)?.Value;
            return value ?? string.Empty;
        }
    }
}

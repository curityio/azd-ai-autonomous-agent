namespace IO.Curity.PortfolioMcpServer.SecurityTests
{
    /*
     * A stock entity, which the example associates to a region
     */
    public class Stock
    {
        public required string Id { get; set; }

        public required string Name { get; set; }

        public required string Region { get; set; }

        public required double CurrentPriceUSD { get; set; }
    }
}

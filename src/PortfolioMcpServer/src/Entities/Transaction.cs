namespace IO.Curity.PortfolioMcpServer.Entities
{
    /*
     * A transaction to buy or sell a stock
     */
    public class Transaction
    {
        public required string CustomerId { get; set; }

        public required DateTime ExecutionDate { get; set; }

        public required string StockID { get; set; }

        public required int Quantity { get; set; }

        public required double UnitPriceUSD { get; set; }
    }
}

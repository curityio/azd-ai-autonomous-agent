namespace IO.Curity.PortfolioMcpServer.Entities
{
    /*
     * A portfolio is a collection of transactions that represent the user's financial investments
     */
    public class Portfolio
    {
        public required Transaction[] Transactions { get; set; }
    }
}

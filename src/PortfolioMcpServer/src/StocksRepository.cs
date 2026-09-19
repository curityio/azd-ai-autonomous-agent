namespace IO.Curity.PortfolioMcpServer
{
    using System;
    using System.Linq;
    using IO.Curity.PortfolioMcpServer.Entities;

    /*
     * Simulate data for a real MCP server that operates on stock transactions
     */
    public sealed class StocksRepository
    {
        static string USA = "USA";
        static string EUROPE = "EUROPE";
        static string ASIA = "ASIA";

        /*
         * Generate an example history of transaction using access token claims
         */
        public Portfolio GetPortfolio(string customerId, string region)
        {
            var stocks = this.GetStocks(region);
            if (stocks.Length < 2)
            {
                return new Portfolio()
                {
                    Transactions = [],
                };
            }
                
            var stock1 = stocks.First(s => s.Region == region);
            var stock2 = stocks.Last(s => s.Region == region);

            Transaction[] customerTransactions =
            [
                new()
                {
                    CustomerId = customerId,
                    ExecutionDate = DateTime.UtcNow.AddDays(-150),
                    StockID = stock1.Id,
                    Quantity = 300,
                    UnitPriceUSD = stock1.CurrentPriceUSD + 40.0,
                },
                new()
                {
                    CustomerId = customerId,
                    ExecutionDate = DateTime.UtcNow.AddDays(-150),
                    StockID = stock2.Id,
                    Quantity = 200,
                    UnitPriceUSD = stock2.CurrentPriceUSD + 30.0,
                },
                new()
                {
                    CustomerId = customerId,
                    ExecutionDate = DateTime.UtcNow.AddDays(-70),
                    StockID = stock2.Id,
                    Quantity = -50,
                    UnitPriceUSD = stock2.CurrentPriceUSD - 15.0,
                },
                new()
                {
                    CustomerId = customerId,
                    ExecutionDate = DateTime.UtcNow.AddDays(-70),
                    StockID = stock1.Id,
                    Quantity = -75,
                    UnitPriceUSD = stock1.CurrentPriceUSD - 10.0,
                },
                new()
                {
                    CustomerId = customerId,
                    ExecutionDate = DateTime.UtcNow.AddDays(-30),
                    StockID = stock1.Id,
                    Quantity = 50,
                    UnitPriceUSD = stock1.CurrentPriceUSD + 10.0,
                },
                new()
                {
                    CustomerId = customerId,
                    ExecutionDate = DateTime.UtcNow.AddDays(-30),
                    StockID = stock2.Id,
                    Quantity = 100,
                    UnitPriceUSD = stock2.CurrentPriceUSD + 7.5,
                },
            ];

            return new Portfolio()
            {
                Transactions = customerTransactions,
            };
        }

        /*
         * Generate example stocks and their current prices to enable a portfolio value
         */
        public Stock[] GetStocks(string region)
        {
            if (region != USA && region != EUROPE && region != ASIA)
            {
                return [];
            }

            Stock[] allStocks =
            [
                new()
                {
                    Id = "COM1",
                    Name = "Company 1",
                    Region = USA,
                    CurrentPriceUSD = 386.54,
                },
                new()
                {
                    Id = "COM2",
                    Name = "Company 2",
                    Region = ASIA,
                    CurrentPriceUSD = 250.62,
                },
                new()
                {
                    Id = "COM3",
                    Name = "Company 3",
                    Region = EUROPE,
                    CurrentPriceUSD = 21.07,
                },
                new()
                {
                    Id = "COM4",
                    Name = "Company 4",
                    Region = USA,
                    CurrentPriceUSD = 180.75,
                },
                new()
                {
                    Id = "COM5",
                    Name = "Company 5",
                    Region = EUROPE,
                    CurrentPriceUSD = 87.50,
                },
                new()
                {
                    Id = "COM6",
                    Name = "Company 6",
                    Region = ASIA,
                    CurrentPriceUSD = 109.88,
                },
            ];

            return allStocks.Where(s => s.Region == region).ToArray();
        }
    }
}

namespace IO.Curity.PortfolioMcpServer
{
    using System;
    using System.Linq;
    using IO.Curity.PortfolioMcpServer.Entities;

    /*
     * The example MCP server uses hard coded data that contains identity attributes
     */
    public sealed class DataRepository
    {
        /*
         * Return mock information about stocks, where different stocks are traded per region
         * Each region has 2 stocks in the mock data
         */
        public Stock[] GetAvailableStocks(string region)
        {
            Stock[] allStocks =
            [
                new()
                {
                    Id = "COM1",
                    Name = "Company 1",
                    Region = "USA",
                    CurrentPriceUSD = 386.54,
                },
                new()
                {
                    Id = "COM2",
                    Name = "Company 2",
                    Region = "Asia",
                    CurrentPriceUSD = 250.62,
                },
                new()
                {
                    Id = "COM3",
                    Name = "Company 3",
                    Region = "Europe",
                    CurrentPriceUSD = 21.07,
                },
                new()
                {
                    Id = "COM4",
                    Name = "Company 4",
                    Region = "USA",
                    CurrentPriceUSD = 180.75,
                },
                new()
                {
                    Id = "COM5",
                    Name = "Company 5",
                    Region = "Europe",
                    CurrentPriceUSD = 87.50,
                },
                new()
                {
                    Id = "COM6",
                    Name = "Company 6",
                    Region = "Asia",
                    CurrentPriceUSD = 109.88,
                },
            ];

            return allStocks.Where(s => s.Region == region).ToArray();
        }

        /*
         * Return the current value of the portfolio for the customer ID in the access token
         */
        public Portfolio GetPortfolio(string customerId, string region)
        {
            // Get stocks for the region
            var stocks = GetAvailableStocks(region);
            var stock1 = stocks.First(s => s.Region == region);
            var stock2 = stocks.Last(s => s.Region == region);

            // Make up some transactions for customers
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
    }
}

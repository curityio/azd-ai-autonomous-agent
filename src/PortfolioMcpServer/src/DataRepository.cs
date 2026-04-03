namespace IO.Curity.PortfolioMcpServer
{
    using System;
    using System.Linq;
    using IO.Curity.PortfolioMcpServer.Entities;

    /*
     * Simulate a real MCP server that operates on stock transactions
     */
    public sealed class DataRepository
    {
        static string usa = "USA";
        static string europe = "Europe";
        static string asia = "Asia";
        
        /*
         * Return some hard coded stocks, where stocks are traded in a particular region
         */
        public Stock[] GetAvailableStocks(string region)
        {
            if (region != usa && region != europe && region != asia)
            {
                return [];
            }

            Stock[] allStocks =
            [
                new()
                {
                    Id = "COM1",
                    Name = "Company 1",
                    Region = usa,
                    CurrentPriceUSD = 386.54,
                },
                new()
                {
                    Id = "COM2",
                    Name = "Company 2",
                    Region = asia,
                    CurrentPriceUSD = 250.62,
                },
                new()
                {
                    Id = "COM3",
                    Name = "Company 3",
                    Region = europe,
                    CurrentPriceUSD = 21.07,
                },
                new()
                {
                    Id = "COM4",
                    Name = "Company 4",
                    Region = usa,
                    CurrentPriceUSD = 180.75,
                },
                new()
                {
                    Id = "COM5",
                    Name = "Company 5",
                    Region = europe,
                    CurrentPriceUSD = 87.50,
                },
                new()
                {
                    Id = "COM6",
                    Name = "Company 6",
                    Region = asia,
                    CurrentPriceUSD = 109.88,
                },
            ];

            return allStocks.Where(s => s.Region == region).ToArray();
        }

        /*
         * A real system would retrieve transactions from a database that match the customer ID and region in the access token
         * This method just generates some demo data to show the effect of an LLM operating on raw data
         */
        public Portfolio GetPortfolio(string customerId, string region)
        {
            var stocks = GetAvailableStocks(region);
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
    }
}

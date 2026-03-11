namespace IO.Curity.PortfolioMcpServer.SecurityTests
{
    using System;

    /*
     * Token settings for a particular test
     */
    public sealed class MockTokenOptions
    {
        public MockTokenOptions(Configuration configuration)
        {
            this.Issuer = configuration.Issuer;
            this.Audience = configuration.Audience;
            this.Scope = configuration.Scope;
            this.ExpiryMinutes = 15;
            this.Subject = Guid.NewGuid().ToString();
            this.CustomerId = string.Empty;
            this.Region = string.Empty;
        }

        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Scope { get; set; }
        public int ExpiryMinutes { get; set; }
        public string Subject { get; set; }
        public string CustomerId { get; set; }
        public string Region { get; set; }
    }
}

namespace IO.Curity.PortfolioMcpServer.SecurityTests
{
    using System;
    using IO.Curity.PortfolioMcpServer.Utilities;

    /*
     * Token settings for a particular test
     */
    public sealed class MockTokenOptions
    {
        /*
         * Set defaults
          */
        public MockTokenOptions(Configuration configuration)
        {
            this.Issuer = configuration.Issuer;
            this.Audience = configuration.Audience;
            this.Scope = configuration.Scope;
            this.ExpiryMinutes = 15;
            this.Subject = Guid.NewGuid().ToString();
            this.CustomerId = string.Empty;
            this.Region = string.Empty;
            this.AgentClaims = null;
            
        }

        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Scope { get; set; }
        public int ExpiryMinutes { get; set; }
        public string Subject { get; set; }
        public string CustomerId { get; set; }
        public string Region { get; set; }
        public AgentClaims? AgentClaims { get; set; }
    }
}

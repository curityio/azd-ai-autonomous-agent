namespace IO.Curity.PortfolioMcpServer.Tests
{
    using System;
    using IO.Curity.PortfolioMcpServer.Entities;
    using Jose;

    /*
     * Token settings for a particular test
     */
    public sealed class MockTokenOptions
    {
        /*
         * Default to working values
          */
        public MockTokenOptions(Configuration configuration)
        {
            this.Issuer = configuration.Issuer;
            this.Audience = configuration.Audience;
            this.SigningKey = null;
            this.Scope = configuration.Scope;
            this.ExpiryMinutes = 15;
            this.Subject = Guid.NewGuid().ToString();
            this.CustomerId = "195";
            this.Region = "EUROPE";
            this.AgentClaims = new AgentClaims()
            {
                AgentId = "example-agent",
                AgentRole = "analyst",
                AgentDepartment = "finance"
            };
        }

        public string Issuer { get; set; }
        public string Audience { get; set; }
        public Jwk? SigningKey { get; set; }
        public string Scope { get; set; }
        public int ExpiryMinutes { get; set; }
        public string Subject { get; set; }
        public string CustomerId { get; set; }
        public string Region { get; set; }
        public AgentClaims? AgentClaims { get; set; }
    }
}

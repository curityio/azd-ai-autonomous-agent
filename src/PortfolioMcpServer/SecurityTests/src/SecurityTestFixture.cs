namespace IO.Curity.PortfolioMcpServer.SecurityTests
{
    using System;
    using Xunit;
    using IO.Curity.PortfolioMcpServer.SecurityTests.Utilities;

    /*
     * Manages setup before running tests and teardown afterwards
     */
    public class SecurityTestFixture : IDisposable
    {
        public Configuration Configuration { get; private set; }
        public MockAccessTokenIssuer AccessTokenIssuer { get; private set; }

        public SecurityTestFixture()
        {
            this.Configuration = new Configuration();
            this.AccessTokenIssuer = new MockAccessTokenIssuer(this.Configuration, TestContext.Current);
        }

        public void Dispose()
        {
            this.AccessTokenIssuer?.Dispose();
        }
    }
}

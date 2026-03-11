namespace IO.Curity.PortfolioMcpServer.SecurityTests
{
    using System;
    using Xunit;

    /*
     * Manages setup before running tests and teardown afterwards
     */
    public class SecurityTestFixture : IDisposable
    {
        public Configuration Configuration { get; private set; }
        public MockAuthorizationServer AuthorizationServer { get; private set; }

        public SecurityTestFixture()
        {
            this.Configuration = new Configuration();
            this.AuthorizationServer = new MockAuthorizationServer(this.Configuration, TestContext.Current);
        }

        public void Dispose()
        {
            this.AuthorizationServer.Dispose();
        }
    }
}
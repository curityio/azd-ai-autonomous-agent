namespace IO.Curity.PortfolioMcpServer.SecurityTests
{
    using System;
    using System.IO;

    /*
     * Load configuration parameters
     */
    public class Configuration
    {
        public int JwksUriPort {get; private set;}
        public string Issuer {get; private set;}
        public string Audience {get; private set;}
        public string Algorithm {get; private set;}
        public string Scope {get; private set;}
        public string PortfolioMcpServerUrl {get; private set;}

        public Configuration()
        {
            this.JwksUriPort = int.Parse(ReadEnvironmentVariable("JWKS_URI_PORT"));
            this.Issuer = ReadEnvironmentVariable("ISSUER");
            this.Audience = ReadEnvironmentVariable("AUDIENCE");
            this.Algorithm = ReadEnvironmentVariable("ALGORITHM");
            this.Scope = ReadEnvironmentVariable("SCOPE");
            this.PortfolioMcpServerUrl = ReadEnvironmentVariable("PORTFOLIO_MCP_SERVER_URL");
        }

        private static string ReadEnvironmentVariable(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidDataException($"Environment variable {name} was not found");
            }

            return value;
        }
    }
}

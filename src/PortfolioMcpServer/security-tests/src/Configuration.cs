namespace IO.Curity.PortfolioMcpServer.SecurityTests
{
    using System;
    using System.IO;

    /*
     * Load configuration parameters
     */
    public class Configuration
    {
        public string PortfolioMcpServerUrl {get; private set;}
        public string JwksUri {get; private set;}
        public string Issuer {get; private set;}
        public string Audience {get; private set;}
        public string Algorithm {get; private set;}
        public string Scope {get; private set;}

        public Configuration()
        {
            this.PortfolioMcpServerUrl = ReadEnvironmentVariable("PORTFOLIO_MCP_SERVER_URL");
            this.JwksUri = ReadEnvironmentVariable("JWKS_URI");
            this.Issuer = ReadEnvironmentVariable("ISSUER");
            this.Audience = ReadEnvironmentVariable("AUDIENCE");
            this.Algorithm = ReadEnvironmentVariable("ALGORITHM");
            this.Scope = ReadEnvironmentVariable("SCOPE");
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

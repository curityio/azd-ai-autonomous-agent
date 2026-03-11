namespace IO.Curity.PortfolioMcpServer
{
    using System;

    /*
     * Load configuration parameters
     */
    public class Configuration
    {
        public bool IsLocalDevelopment {get; private set;}
        public int Port {get; private set;}
        public string ExternalBaseUrl {get; private set;}
        public string AuthorizationServerBaseUrl {get; private set;}
        public string Issuer {get; private set;}
        public string Audience {get; private set;}
        public string Algorithm {get; private set;}
        public string Scope {get; private set;}
        public string JwksUri {get; private set;}


        public Configuration()
        {
            this.IsLocalDevelopment = ReadEnvironmentVariable("ENV") == "local";
            this.Port = int.Parse(ReadEnvironmentVariable("PORT"));
            this.ExternalBaseUrl = ReadEnvironmentVariable("EXTERNAL_BASE_URL");
            this.AuthorizationServerBaseUrl = ReadEnvironmentVariable("AUTHORIZATION_SERVER_BASE_URL");
            this.Issuer = ReadEnvironmentVariable("ISSUER");
            this.Audience = ReadEnvironmentVariable("AUDIENCE");
            this.Algorithm = ReadEnvironmentVariable("ALGORITHM");
            this.Scope = ReadEnvironmentVariable("SCOPE");
            this.JwksUri = ReadEnvironmentVariable("JWKS_URI", false);
        }

        private static string ReadEnvironmentVariable(string name, bool required = true)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value) && required)
            {
                throw new InvalidDataException($"Environment variable {name} was not found");
            }

            return value ?? string.Empty;
        }
    }
}

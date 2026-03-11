namespace IO.Curity.AutonomousAgent
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
        public string Issuer {get; private set;}
        public string Audience {get; private set;}
        public string Algorithm {get; private set;}
        public string AuthorizationUrl {get; private set;}
        public string TokenUrl {get; private set;}
        public string Scope {get; private set;}
        public string TokenExchangeClientId {get; private set;}
        public string TokenExchangeClientSecret {get; private set;}
        public string TokenExchangeTargetAudience {get; private set;}
        public int TokenExchangeCacheSeconds  {get; private set;}
        public string PortfolioMcpServerUrl {get; private set;}
        public string AzureFoundryProjectUrl {get; private set;}
        public string AzureAIModelName {get; private set;}
        public string ManagedIdentityClientId {get; private set;}

        public Configuration()
        {
            this.IsLocalDevelopment = ReadEnvironmentVariable("ENV") == "local";
            this.Port = int.Parse(ReadEnvironmentVariable("PORT"));
            this.ExternalBaseUrl = ReadEnvironmentVariable("EXTERNAL_BASE_URL");
            this.Issuer = ReadEnvironmentVariable("ISSUER");
            this.Audience = ReadEnvironmentVariable("AUDIENCE");
            this.Algorithm = ReadEnvironmentVariable("ALGORITHM");
            this.AuthorizationUrl = ReadEnvironmentVariable("AUTHORIZATION_URL");
            this.TokenUrl = ReadEnvironmentVariable("TOKEN_URL");
            this.Scope = ReadEnvironmentVariable("SCOPE");
            this.TokenExchangeClientId = ReadEnvironmentVariable("TOKEN_EXCHANGE_CLIENT_ID");
            this.TokenExchangeClientSecret = ReadEnvironmentVariable("TOKEN_EXCHANGE_CLIENT_SECRET");
            this.TokenExchangeTargetAudience = ReadEnvironmentVariable("TOKEN_EXCHANGE_TARGET_AUDIENCE");
            this.TokenExchangeCacheSeconds = int.Parse(ReadEnvironmentVariable("TOKEN_EXCHANGE_CACHE_SECONDS"));
            this.PortfolioMcpServerUrl = ReadEnvironmentVariable("PORTFOLIO_MCP_SERVER_URL");
            this.AzureFoundryProjectUrl = ReadEnvironmentVariable("AZURE_AI_FOUNDRY_PROJECT_URL");
            this.AzureAIModelName = ReadEnvironmentVariable("AZURE_AI_MODEL_NAME");
            this.ManagedIdentityClientId = ReadEnvironmentVariable("MANAGED_IDENTITY_CLIENT_ID", false);
        }

        private static string ReadEnvironmentVariable(string name, bool required=false)
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

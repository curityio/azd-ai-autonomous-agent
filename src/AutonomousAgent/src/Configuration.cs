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
        public string AuthorizationUrl {get; private set;}
        public string TokenUrl {get; private set;}
        public string Scope {get; private set;}
        public string PortfolioMcpServerUrl {get; private set;}
        public string AzureFoundryProjectUrl {get; private set;}
        public string AzureAIModelDeploymentName {get; private set;}

        public Configuration()
        {
            this.IsLocalDevelopment = ReadEnvironmentVariable("ENV") == "local";
            this.Port = int.Parse(ReadEnvironmentVariable("PORT"));
            this.ExternalBaseUrl = ReadEnvironmentVariable("EXTERNAL_BASE_URL");
            this.AuthorizationUrl = ReadEnvironmentVariable("AUTHORIZATION_URL");
            this.TokenUrl = ReadEnvironmentVariable("TOKEN_URL");
            this.Scope = ReadEnvironmentVariable("SCOPE");
            this.PortfolioMcpServerUrl = ReadEnvironmentVariable("PORTFOLIO_MCP_SERVER_URL");
            this.AzureFoundryProjectUrl = ReadEnvironmentVariable("AZURE_AI_FOUNDRY_PROJECT_URL");
            this.AzureAIModelDeploymentName = ReadEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME");
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

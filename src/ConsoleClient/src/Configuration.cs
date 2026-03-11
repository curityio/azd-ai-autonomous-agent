namespace IO.Curity.ConsoleClient
{
    using System;

    /*
     * Load configuration parameters
     */
    public class Configuration
    {
        public string AutonomousAgentUrl {get; private set;}
        public string ClientId {get; private set;}
        public string Scope {get; private set;}
        public string PostLoginWebsiteUrl {get; private set;}

        public Configuration()
        {
            this.AutonomousAgentUrl = ReadEnvironmentVariable("AUTONOMOUS_AGENT_URL");
            this.ClientId = ReadEnvironmentVariable("CLIENT_ID");
            this.Scope = ReadEnvironmentVariable("SCOPE");
            this.PostLoginWebsiteUrl = ReadEnvironmentVariable("POST_LOGIN_WEBSITE_URL");
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

namespace IO.Curity.ConsoleClient
{
    using System;
    using System.Text.Json;
    using A2A;
    using IO.Curity.ConsoleClient.Security;

    /*
     * The example program is a simple A2A console app, and you could use the same approach in a web or mobile app
     */
    public static class Program
    {
        /*
         * Get agent metadata, authenticate the user and call A2A entry points
         */
        public static async Task Main()
        {
            try
            {
                // Load configuration
                var configuration = new Configuration();

                // Download the autonomous agent's A2A agent card
                var agentUrl = new Uri(configuration.AutonomousAgentUrl);
                
                Console.WriteLine("Downloading A2A agent card metadata ...");
                var cardResolver = new A2ACardResolver(baseUrl: agentUrl, agentCardPath: $"{agentUrl.AbsolutePath}/.well-known/agent-card.json");
                var agentCard = await cardResolver.GetAgentCardAsync();

                // Create an OAuth Client that uses the OAuth security scheme of the A2A server and authenticate the user
                var oauthInfo = agentCard?.SecuritySchemes?.FirstOrDefault(s => s.Key == "oauth2").Value;
                var oauthClient = new OAuthClient(configuration, oauthInfo);
                Console.WriteLine("Authenticating the user, to get an access token ...");
                await oauthClient.LoginAsync();

                Console.WriteLine("Sending a natural language command with an access token ...");
                var userCommand = 
                    "Give me a markdown report on the last 3 months of stock transactions and the value of my portfolio";
                Console.WriteLine($"- {userCommand}");
                var agentClient = new AgentClient(agentUrl, oauthClient);
                var agentResponse = await agentClient.SendNaturalLanguageCommandAsync(userCommand);
                Console.WriteLine(agentResponse);
            }
            catch (ClientError error)
            {
                // Report error details in a JSON format
                var json = JsonSerializer.Serialize(error.ToJson(), new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(json);
            }
        }
    }
}

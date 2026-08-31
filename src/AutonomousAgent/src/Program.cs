namespace IO.Curity.AutonomousAgent
{
    using System.Net;
    using A2A.AspNetCore;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using IO.Curity.AutonomousAgent.Utilities;

    /*
     * The entry point for the autonomous agent
     */
    public static class Program
    {
        /*
         * Create the autonomous agent as an A2A service
         */
        public static async Task Main()
        {
            var configuration = new Configuration();
            
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.AddJsonFile("appSettings.json");
            builder.WebHost
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, configuration.Port);
                });

            
            builder.Services.AddA2AAgent<AutonomousAgent>(AutonomousAgent.GetAgentCard(configuration));
            builder.Services.AddSingleton(configuration);
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<LlmHttpClientPolicy>();
            builder.Services.AddSingleton<McpHttpClientHandler>();

            var app = builder.Build();
            app.UseRouting();
            app.MapA2A(path: "/");
            app.MapWellKnownAgentCard(AutonomousAgent.GetAgentCard(configuration));
            app.Run();
        }
    }
}

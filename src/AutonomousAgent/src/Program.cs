namespace IO.Curity.AutonomousAgent
{
    using System.Net;
    using A2A.AspNetCore;
    using IO.Curity.AutonomousAgent.Security;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;

    /*
     * The entry point for the autonomous agent
     */
    public static class Program
    {
        /*
         * The agent is an A2A service, where A2A endpoints are protected by JWT access tokens
         */
        public static async Task Main()
        {
            // Load configuration settings
            var configuration = new Configuration();
            
            // The agent can log OAuth error details but does not return them to the caller
            IdentityModelEventSource.ShowPII = configuration.IsLocalDevelopment;

            // The MCP server runs in an internal network
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.AddJsonFile("appSettings.json");
            builder.WebHost
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, configuration.Port);
                });

            // The agent validates a JWT access token on every request, to protect access to the Azure LLM, and uses audience restrictions
            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration.Issuer;
                    options.Audience = configuration.Audience;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAlgorithms = [configuration.Algorithm],
                    };
                    options.RequireHttpsMetadata = false;
                    options.MapInboundClaims = false;
                });
            
            builder.Services.AddAuthorization(options =>
            {
                // All endpoints require JWTs except the agent card endpoint
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new AllowAnonymousAgentCardRequirement())
                    .Build();

                // Authorized endpoints check for the agent's required scope
                options.AddPolicy("scope", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim =>
                            claim.Type == "scope" && claim.Value.Split(' ').Any(c => c == configuration.Scope)
                        )
                    )
                );
            });

            // Expose endpoints as an A2A server over HTTP
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDistributedMemoryCache();

            // Add the agent
            builder.Services.AddA2AAgent<AutonomousAgent>(AutonomousAgent.GetAgentCard(configuration));

            // Define injectable objects
            builder.Services.AddSingleton(configuration);
            builder.Services.AddSingleton<AIAgentFactory>();
            builder.Services.AddSingleton<OAuthHttpClientHandler>();
            builder.Services.AddSingleton<TokenExchangeClient>();
            builder.Services.AddSingleton<TokenCache>();

            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();

            // Create and run the A2A server as a web API
            var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
            var oauthHttpClientHandler = app.Services.GetRequiredService<OAuthHttpClientHandler>();
            var agent = new AutonomousAgent(configuration, oauthHttpClientHandler, loggerFactory);
            
            // Map A2A paths and apply a policy to check for the required scope
            app.MapA2A(path: "/").RequireAuthorization("scope");
            app.Run();
        }
    }
}

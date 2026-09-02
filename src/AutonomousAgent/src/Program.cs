namespace IO.Curity.AutonomousAgent
{
    using System.Net;
    using A2A.AspNetCore;
    using IO.Curity.AutonomousAgent.Security;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;

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
            IdentityModelEventSource.ShowPII = configuration.IsLocalDevelopment;
            
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.AddJsonFile("appSettings.json");
            builder.WebHost
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, configuration.Port);
                });

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
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new AllowAnonymousAgentCardRequirement())
                    .Build();

                options.AddPolicy("scope", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim =>
                            claim.Type == "scope" && claim.Value.Split(' ').Any(c => c == configuration.RequiredScope)
                        )
                    )
                );
            });

            builder.Services.AddA2AAgent<AutonomousAgent>(AutonomousAgent.GetAgentCard(configuration));
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSingleton(configuration);
            builder.Services.AddSingleton<OAuthHttpClientHandler>();
            builder.Services.AddSingleton<TokenExchangeClient>();
            builder.Services.AddSingleton<TokenCache>();

            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapA2A(path: "/").RequireAuthorization("scope");
            app.MapWellKnownAgentCard(AutonomousAgent.GetAgentCard(configuration));
            app.Run();
        }
    }
}

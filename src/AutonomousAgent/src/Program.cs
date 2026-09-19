namespace IO.Curity.AutonomousAgent
{
    using System.Net;
    using A2A.AspNetCore;
    using IO.Curity.AutonomousAgent.Security;
    using IO.Curity.AutonomousAgent.Utilities;
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
                    
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            
                            var error = ErrorFactory.CreateUnauthorizedError();
                            context.Response.Headers.WWWAuthenticate = $"Bearer error=\"{error.Code}\", error_description=\"{error.Message}\"";

                            await context.Response.WriteAsJsonAsync(new
                            {
                                error = error.Code,
                                error_description = error.Message,
                            });
                        },
                        OnForbidden = async context =>
                        {
                            var error = "insufficient_scope";
                            var description = "The access token has insufficient privileges";

                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/json";
                            context.Response.Headers.WWWAuthenticate = $"Bearer error=\"{error}\", error_description=\"{description}\"";

                            await context.Response.WriteAsJsonAsync(new
                            {
                                error,
                                error_description = description
                            });
                        },
                    };
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

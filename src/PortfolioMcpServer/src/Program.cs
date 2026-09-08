namespace IO.Curity.PortfolioMcpServer
{
    using System.Net;
    using System.Threading.Tasks;
    using IO.Curity.PortfolioMcpServer.Utilities;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Logging;
    using Microsoft.IdentityModel.Tokens;

    /*
     * The entry point for the MCP server
     */
    public static class Program
    {
        /*
         * The MCP server's endpoints are protected by JWT access tokens
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
                        ValidIssuer = configuration.Issuer,
                        ValidAlgorithms = [configuration.Algorithm],
                    };
                    options.RequireHttpsMetadata = false;
                    options.MapInboundClaims = false;
                    options.RequireHttpsMetadata = false;
                    options.MapInboundClaims = false;

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();

                            var error = "invalid_token";
                            var description = "The access token is missing, invalid, or expired";

                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";
                            context.Response.Headers.WWWAuthenticate = $"Bearer error=\"{error}\", error_description=\"{description}\"";

                            await context.Response.WriteAsJsonAsync(new
                            {
                                error,
                                error_description = description
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

                    options.TokenValidationParameters.IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                    {
                        var httpClient = new HttpClient();
                        var response = httpClient.GetStringAsync(configuration.JwksUri).Result;
                        var keys = new JsonWebKeySet(response).GetSigningKeys();
                        var matchingKeys = keys.Where(key => key.KeyId == kid).ToList();
                        if (matchingKeys.Count == 0)
                        {
                            throw new SecurityTokenException($"The kid {kid} in the JWT header was not found");
                        }

                        return matchingKeys;
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

                options.AddPolicy("scope", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim =>
                            claim.Type == "scope" && claim.Value.Split(' ').Any(c => c == configuration.RequiredScope)
                        )
                    )
                );

                options.AddPolicy("agent", policy =>
                    policy.RequireAssertion(context =>
                    {
                        var agentClaims = context.User.GetAgentClaims();
                        if (agentClaims == null)
                        {
                            return false;
                        }

                        return agentClaims.AgentDepartment == "finance";
                    }));
            });

            builder.Services.AddSingleton(configuration);
            builder.Services.AddSingleton(new StocksRepository());

            builder.Services.AddControllers();
            builder.Services
                .AddMcpServer()
                .WithHttpTransport()
                .WithTools<StocksToolsService>();

            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapMcp().RequireAuthorization("scope", "agent");
            app.Run();
        }
    }
}

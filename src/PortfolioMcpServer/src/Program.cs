namespace IO.Curity.PortfolioMcpServer
{
    using System.Net;
    using System.Threading.Tasks;
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
            // Load configuration settings
            var configuration = new Configuration();
            
            // The MCP server can log OAuth error details but does not return them to the caller
            IdentityModelEventSource.ShowPII = true;

            // The MCP server runs in an internal network
            var builder = WebApplication.CreateBuilder();
            builder.Configuration.AddJsonFile("appSettings.json");
            builder.WebHost
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, configuration.Port);
                });

            // The MCP server validates a JWT access token on every request and uses audience restrictions
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

                    // This example uses an explicit JWKS URI that can be overridden for testing
                    if (!string.IsNullOrWhiteSpace(configuration.JwksUri))
                    {
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
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                // All endpoints require JWTs except the resource metadata endpoint which uses [AllowAnonymous]
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

                // Authorized endpoints check for the MCP server's required scope
                options.AddPolicy("scope", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim =>
                            claim.Type == "scope" && claim.Value.Split(' ').Any(c => c == configuration.Scope)
                        )
                    )
                );
            });

            // Add injectable objects
            builder.Services.AddSingleton(configuration);
            builder.Services.AddSingleton(new DataRepository());

            // Expose endpoints as an MCP server over HTTP
            builder.Services.AddControllers();
            builder.Services
                .AddMcpServer()
                .WithHttpTransport()
                .WithTools<StocksToolsService>();

            // Run the MCP server as a web API
            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapMcp().RequireAuthorization("scope");
            app.Run();
        }
    }
}

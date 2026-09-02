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
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

                options.AddPolicy("scope", policy =>
                {   
                    policy.RequireAssertion(context =>
                    {
                        var receivedScopes = context.User
                            .FindAll("scope")
                            .SelectMany(c => c.Value.Split(' '));
                        
                        return configuration.RequiredScopes.All(scope => receivedScopes.Contains(scope));
                    });
                });
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
            app.MapMcp().RequireAuthorization("scope");
            app.Run();
        }
    }
}

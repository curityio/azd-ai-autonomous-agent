namespace IO.Curity.PortfolioMcpServer.Utilities
{
    using System.Security.Claims;
    using System.Text.Json;

    /*
     * Read the act claim from the access token to get agent claims
     */
    public static class ClaimsPrincipalExtensions
    {
        public static AgentClaims? GetAgentClaims(this ClaimsPrincipal principal)
        {
            var actClaim = principal.FindFirst("act")?.Value;
            if (string.IsNullOrWhiteSpace(actClaim))
                return null;

            return JsonSerializer.Deserialize<AgentClaims>(actClaim);
        }
    }
}

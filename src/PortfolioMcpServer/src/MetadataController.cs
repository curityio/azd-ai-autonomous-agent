namespace IO.Curity.PortfolioMcpServer
{
    using System.Text.Json.Nodes;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /*
     * Implement an OAuth protected resource endpoint that can trigger OAuth flows
     */
    public class MetadataController : Controller
    {
        private readonly Configuration configuration;

        public MetadataController(Configuration configuration)
        {
            this.configuration = configuration;
        }

        /*
         * The MCP server provides protected resource metadata
         */
        [AllowAnonymous]
        [HttpGet(".well-known/oauth-protected-resource")]
        public JsonNode GetPortfolio()
        {
            return new JsonObject
            {
                ["resource"] = this.configuration.ExternalBaseUrl,
                ["resource_name"] = "Portfolio MCP Server",
                ["authorization_servers"] =  this.configuration.AuthorizationServerBaseUrl,
                ["scopes_supported"] = this.configuration.Scope,
            };
        }
    }
}

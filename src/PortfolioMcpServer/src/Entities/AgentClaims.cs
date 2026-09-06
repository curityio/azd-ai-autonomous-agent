namespace IO.Curity.PortfolioMcpServer.Entities
{
    using System.Text.Json.Serialization;

    /*
     * The MCP server receives these properties about the calling agent in the access token
     */
    public class AgentClaims
    {
        [JsonPropertyName("sub")]
        public string? AgentId { get; init; }
        
        [JsonPropertyName("agent_role")]
        public string? AgentRole { get; init; }
        
        [JsonPropertyName("agent_department")]
        public string? AgentDepartment { get; init; }
    }
}

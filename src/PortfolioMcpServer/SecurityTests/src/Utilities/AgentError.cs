namespace IO.Curity.PortfolioMcpServer.SecurityTests.Utilities
{
    using System;
    using System.Net;
    using System.Text.Json.Nodes;

    /*
     * Basic error handling for remote requests
     */
    public class McpError : Exception
    {
        public readonly string Code;
        public readonly HttpStatusCode StatusCode;

        public McpError(HttpStatusCode statusCode, string code, string message) : base(message)
        {
            this.Code = code;
            this.StatusCode = statusCode;
        }

        public JsonNode ToJson()
        {
            return new JsonObject
            {
                ["error"] = this.Code,
                ["error_description"] = this.Message,
                ["status"] = (int)this.StatusCode,
            };
        }
    }
}

namespace IO.Curity.AutonomousAgent.Utilities
{
    using System;
    using System.Text.Json.Nodes;

    /*
     * Basic error handling for remote requests
     */
    public class AgentError : Exception
    {
        public readonly string Code;
        public readonly int StatusCode;

        public AgentError(int statusCode, string code, string message) : base(message)
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
                ["status"] = this.StatusCode,
            };
        }
    }
}

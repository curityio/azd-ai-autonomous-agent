namespace IO.Curity.ConsoleClient
{
    using System;
    using System.Text.Json.Nodes;

    /*
     * Basic error handling for remote requests
     */
    public class ClientError : Exception
    {
        private readonly string code;

        public ClientError(string code, string message) : base(message)
        {
            this.code = code;
            this.StatusCode = 0;
        }

        public int StatusCode { get; set; }

        public JsonNode ToJson()
        {
            var data = new JsonObject
            {
                ["error"] = this.code,
                ["error_description"] = this.Message,
            };

            if (this.StatusCode != 0)
            {
                data["status"] = this.StatusCode;
            }

            return data;
        }
    }
}

namespace IO.Curity.ConsoleClient
{
    using System;
    using System.Text.Json.Nodes;

    /*
     * Basic error handling for remote OAuth and A2A remote requests
     */
    public class ClientError : Exception
    {
        private readonly string code;

        public ClientError(string code, string message, Exception? cause = null) : base(message, cause)
        {
            this.code = code;
            this.StatusCode = 0;
        }

        public int StatusCode { get; set; }

        public JsonNode ToJson()
        {
            var data = new JsonObject
            {
                ["code"] = this.code,
                ["message"] = this.Message,
            };

            if (this.StatusCode != 0)
            {
                data["status"] = this.StatusCode;
            }

            if (this.InnerException != null)
            {
                data["detail"] = this.InnerException.Message;
            }

            return data;
        }
    }
}

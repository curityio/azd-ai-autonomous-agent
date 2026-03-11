namespace IO.Curity.AutonomousAgent
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;

    /*
     * Helper methods to deal with requests
     */
    public static class HttpRequestExtensions
    {
        /*
         * Return a header or a default value
         */
        public static string GetHeader(this HttpRequest request, string name)
        {
            if (request.Headers != null)
            {
                if (request.Headers.TryGetValue(name, out StringValues value))
                {
                    return value.ToString();
                }
            }

            return string.Empty;
        }
    }
}
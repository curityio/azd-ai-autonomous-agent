namespace IO.Curity.AutonomousAgent.Utilities
{
    using Microsoft.AspNetCore.Http;

    /*
     * Helper methods to deal with HTTP context
     */
    public static class HttpContextExtensions
    {
        /*
         * Get the received access token from the external client that sent a secured A2A request
         */
        public static string GetAccessToken(this IHttpContextAccessor httpContextAccessor)
        {
            var authorization = httpContextAccessor.HttpContext?.Request.GetHeader("authorization");
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var parts = authorization.Split(' ');
                if (parts.Length == 2 && parts[0].ToLowerInvariant() == "bearer")
                {
                   return parts[1];
                }
            }

            return string.Empty;
        }
    }
}

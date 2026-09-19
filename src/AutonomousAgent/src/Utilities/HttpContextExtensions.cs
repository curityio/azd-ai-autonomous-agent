namespace IO.Curity.AutonomousAgent.Utilities
{
    using Microsoft.AspNetCore.Http;

    /*
     * Helper methods to deal with HTTP context
     */
    public static class HttpContextExtensions
    {
        /*
         * Get the received JWT access token
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

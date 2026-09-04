namespace IO.Curity.AutonomousAgent.Utilities
{
    /*
     * Create simple client errors
     */
    public class ErrorFactory
    {
        /*
         * If the agent's access token is rejected, inform the client
         */
        public static AgentError CreateUnauthorizedError()
        {
            return new AgentError(401, "invalid_token", "Missing, invalid or expired access token");
        }

        /*
         * If the agent's access token has insufficient privileges it may be a runtime update
         */
        public static AgentError CreateForbiddenError()
        {
            return new AgentError(403, "insufficient_scope", "The access token has insufficient privileges");
        }

        /*
         * Report general server errors
         */
        public static AgentError CreateServerError()
        {
            return new AgentError(500, "server_error", "Server problem encountered");
        }
    }
}

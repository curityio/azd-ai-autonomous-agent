namespace IO.Curity.AutonomousAgent.Utilities
{
    /*
     * Create simple client errors
     */
    public class ErrorFactory
    {
        public static AgentError CreateUnauthorizedError()
        {
            return new AgentError(401, "invalid_token", "Missing, invalid or expired access token");
        }

        public static AgentError CreateServerError()
        {
            return new AgentError(500, "server_error", "Server problem encountered");
        }
    }
}

namespace IO.Curity.AutonomousAgent.Utilities
{
    using System.ClientModel.Primitives;

    /*
     * An HTTP handler to add credentials to outgoing MCP requests
     */
    public sealed class LlmHttpClientPolicy : PipelinePolicy
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public LlmHttpClientPolicy(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public override void Process(
            PipelineMessage message,
            IReadOnlyList<PipelinePolicy> pipeline,
            int currentIndex)
        {
            message.Request.Headers.Add(
                "Authorization",
                $"Bearer ${this.httpContextAccessor.GetAccessToken()}"
            );

            ProcessNext(message, pipeline, currentIndex);
        }

        public override async ValueTask ProcessAsync(
            PipelineMessage message,
            IReadOnlyList<PipelinePolicy> pipeline,
            int currentIndex)
        {
            System.Console.WriteLine("*** HTTP REQUEST ***");
            System.Console.WriteLine(message.Request.Uri);
            System.Console.WriteLine(this.httpContextAccessor.GetAccessToken());
            message.Request.Headers.Add(
                "Authorization",
                $"Bearer ${this.httpContextAccessor.GetAccessToken()}"
            );

            await ProcessNextAsync(message, pipeline, currentIndex);
        }
    }
}

namespace IO.Curity.PortfolioMcpServer.SecurityTests.Utilities
{
    using Xunit.Runner.Common;
    using Xunit.Sdk;

    /*
     * Display passed tests in green and failed tests in red
     */
    public class CustomReporterMessageHandler : TestMessageSink, IRunnerReporterMessageHandler
    {
        private readonly IRunnerLogger logger;
        private readonly MessageMetadataCache metadataCache;

        public CustomReporterMessageHandler(IRunnerLogger logger)
        {
            this.logger = logger;
            this.metadataCache = new MessageMetadataCache();
            Execution.TestStartingEvent += OnTestStarting;
            Execution.TestPassedEvent += OnTestPassed;
            Execution.TestFailedEvent += OnTestFailed;
        }

        private void OnTestStarting(MessageHandlerArgs<ITestStarting> args)
        {
            this.metadataCache.Set(args.Message);
        }

        private void OnTestPassed(MessageHandlerArgs<ITestPassed> args)
        {
            var green = "\u001b[32m";
            var test = args.Message;
            var metadata = this.metadataCache.TryGetTestMetadata(test);
            if (metadata != null)
            {
                logger.LogMessage($"    {green}[SecurityTests] >> {metadata.TestDisplayName} PASSED ✓");
            }
        }

        private void OnTestFailed(MessageHandlerArgs<ITestFailed> args)
        {
            var red = "\u001b[31m";
            var test = args.Message;
            var metadata = this.metadataCache.TryGetTestMetadata(test);
            if (metadata != null)
            {
                logger.LogMessage($"    {red}[SecurityTests] >> {metadata.TestDisplayName} FAILED ✗: {string.Join(',', test.Messages)}");
            }
        }
    }
}

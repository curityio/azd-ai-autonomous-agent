using Xunit;
using Xunit.Runner.Common;
using IO.Curity.PortfolioMcpServer.SecurityTests.Utilities;

[assembly: TestMethodOrderer(typeof(SequentialTestMethodOrderer))]
[assembly: RegisterRunnerReporter(typeof(CustomReporter))]

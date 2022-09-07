using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using SwiftUserManagement.Infrastructure.Repositories;
using FluentResults;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace SwiftUserManagement.Infrastructure.Test.StepDefinitions
{
    [Binding]
    public class GameAnalysisRabbitMQStepDefinitions
    {
        private readonly Mock<ILogger<RabbitMQFactory>> _logger = new();
        private readonly Mock<IConfiguration> _configuration = new();
        private int result0;
        private int result1;
        private string emitGameAnalysisResult;

        public static string RunCommand(string arguments)
        {
            var output = string.Empty;
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    Verb = "runas",
                    FileName = "cmd.exe",
                    Arguments = "/C " + arguments,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = false
                };

                var proc = Process.Start(startInfo);

                proc.WaitForExit(60000);

                return output;
            }
            catch (Exception)
            {
                return output;
            }
        }

        [BeforeTestRun]
        public static void SetupRabbitMQ()
        {
            RunCommand("docker run -d --hostname rabbitmq-test --name rabbitmq-test -p 5672:5672 -p 15672:15672 rabbitmq:3-management");
            Thread.Sleep(25000); // Wait for the RabbitMQ image to spin up on docker
        }

        [Given(@"The results to be sent through are valid")]
        public void GivenTheResultsToBeSentThroughAreValid(Table table)
        {
            // Arrange
            var row = table.Rows[0];
            result0 = Convert.ToInt32(row[0]);
            result1 = Convert.ToInt32(row[1]);
        }

        [Given(@"The results to be sent through are invalid")]
        public void GivenTheResultsToBeSentThroughAreInvalid(Table table)
        {
            // Arrange
            var row = table.Rows[0];
            result0 = Convert.ToInt32(row[0]);
            result1 = Convert.ToInt32(row[1]);
        }

        [When(@"The Emit Game analysis function is called")]
        public async void WhenTheEmitGameAnalysisFunctionIsCalled()
        {
            // Arrange 
            var inMemorySettings = new Dictionary<string, string> {
                {"RabbitMQSettings:Host", "localhost"},
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            RabbitMQFactory rabbitMQFactory = new RabbitMQFactory(_logger.Object, configuration);

            // Act
            emitGameAnalysisResult = await rabbitMQFactory.EmitGameAnalysis(result0, result1);
        }

        [When(@"The Emit Game analysis function is called with the wrong host")]
        public async void WhenTheEmitGameAnalysisFunctionIsCalledWithTheWrongHost()
        {
            // Arrange 
            var inMemorySettings = new Dictionary<string, string> {
                {"RabbitMQSettings:Host", "rabbit"},
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            RabbitMQFactory rabbitMQFactory = new RabbitMQFactory(_logger.Object, configuration);

            // Act
            emitGameAnalysisResult = await rabbitMQFactory.EmitGameAnalysis(result0, result1);
        }

        [Then(@"The result from this method call should be true")]
        public void ThenTheResultFromThisMethodCallShouldBeTrue()
        {
            // Assert
            emitGameAnalysisResult.Should().Be("Sent game results for analysis");
        }

        [Then(@"The result from this method call should be false")]
        public void ThenTheResultFromThisMethodCallShouldBeFalse()
        {
            // Assert
            emitGameAnalysisResult.Should().Be("Invalid result");
        }

        [Then(@"the result from this method call should return a connection error")]
        public void ThenTheResultFromThisMethodCallShouldReturnAConnectionError()
        {
            emitGameAnalysisResult.Should().Be("Connection error");
        }


        [AfterTestRun]
        public static void TearDownRabbitMQ()
        {
            RunCommand("docker stop rabbitmq");

            RunCommand("docker rm rabbitmq");
        }
    }
}

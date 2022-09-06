using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using SwiftUserManagement.Infrastructure.Repositories;
using FluentResults;

namespace SwiftUserManagement.Infrastructure.Test.StepDefinitions
{
    [Binding]
    public class GameAnalysisRabbitMQStepDefinitions
    {
        private readonly Mock<ILogger<RabbitMQFactory>> _logger = new();
        private int result0;
        private int result1;
        private bool emitGameAnalysisResult;

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
            RabbitMQFactory rabbitMQFactory = new RabbitMQFactory(_logger.Object);

            // Act
            emitGameAnalysisResult = await rabbitMQFactory.EmitGameAnalysis(result0, result1);
        }

        [Then(@"The result from this method call should be true")]
        public void ThenTheResultFromThisMethodCallShouldBeTrue()
        {
            // Assert
            emitGameAnalysisResult.Should().BeTrue();
        }

        [Then(@"The result from this method call should be false")]
        public void ThenTheResultFromThisMethodCallShouldBeFalse()
        {
            // Assert
            emitGameAnalysisResult.Should().BeFalse();
        }
    }
}

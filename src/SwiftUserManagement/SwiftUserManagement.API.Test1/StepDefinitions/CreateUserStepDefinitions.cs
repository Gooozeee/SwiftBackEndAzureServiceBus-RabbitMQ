using FluentResults;
using MassTransit.Mediator;
using Moq;
using Npgsql;

namespace SwiftUserManagement.API.Test1.StepDefinitions
{
    [Binding]
    public class CreateUserStepDefinitions
    {
        public readonly Mock<IMediator> _mediator = new();

        [Given(@"The database is up and running")]
        public void GivenTheDatabaseIsUpAndRunning()
        {
            // Arrange
            try
            {
                using var connection = new NpgsqlConnection
                        ("Server=localhost;Port=5432;Database=UsersDb;User Id=admin;Password=admin1234");

                // Act
                connection.Open();

                // Assert
                connection.Should().NotBeNull();
            }
            catch (Exception e)
            {
                Result.Fail($"Connection to database failed with exception {e}");
            }
        }

        [When(@"I hit the create user endpoint")]
        public void WhenIHitTheCreateUserEndpoint()
        {
            // Arrange

            // Act

            // Assert
        }

        [Then(@"The user will be created")]
        public void ThenTheUserWillBeCreated()
        {
            throw new PendingStepException();
        }
    }
}

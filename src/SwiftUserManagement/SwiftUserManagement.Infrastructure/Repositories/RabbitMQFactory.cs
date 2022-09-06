using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SwiftUserManagement.Application.Contracts.Infrastructure;
using SwiftUserManagement.Domain.Entities;
using System.Text;
using System.Text.Json;

namespace SwiftUserManagement.Infrastructure.Repositories
{
    // Concrete class for emitting tasks out to the rabbitMQ queue
    public class RabbitMQFactory : IMassTransitFactory
    {
        private readonly ILogger<RabbitMQFactory> _Logger;

        public RabbitMQFactory(ILogger<RabbitMQFactory> ILogger)
        {
            _Logger = ILogger ?? throw new ArgumentNullException(nameof(ILogger));
        }

        // Sending out the game score analysis task to the queue
        public Task<bool> EmitGameAnalysis(int result1, int result2)
        {
            if (result1 == 0)
            {
                return Task.FromResult(false);
            }

            // Connecting to the RabbitMQ queue
            try
            {
                var factory = new ConnectionFactory() { HostName = "rabbitmq" };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    _Logger.LogInformation("Sending game results for analysis");

                    // Setting up and sending the message
                    channel.ExchangeDeclare(exchange: "swift_rehab_app",
                                            type: "topic");

                    var routingKey = "game.score.fromApp";
                    var gameResults = new GameResults(result1, result2);
                    var message = JsonSerializer.Serialize(gameResults);
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "swift_rehab_app",
                                         routingKey: routingKey,
                                         basicProperties: null,
                                         body: body);
                    _Logger.LogInformation("Sent game results for analysis");

                    return Task.FromResult(true);
                }
            }
            catch (Exception e)
            {
                _Logger.LogInformation($"Can't connect to RabbitMQ: {e.Message}");
                return Task.FromResult(false);
            }
            
        }

        public async Task<bool> EmitVideonalysis(IFormFile video)
        {
            if (video == null)
            {
                return false;
            }

            // Connecting to the RabbitMQ queue
            try
            {
                var factory = new ConnectionFactory() { HostName = "rabbitmq" };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    _Logger.LogInformation("Sending video file for analysis");

                    // Setting up and sending the message
                    channel.ExchangeDeclare(exchange: "swift_rehab_app",
                                            type: "topic");

                    var routingKey = "video.fromApp";

                    MemoryStream ms = new MemoryStream(new byte[video.Length]);
                    await video.CopyToAsync(ms);

                    channel.BasicPublish(exchange: "swift_rehab_app",
                                         routingKey: routingKey,
                                         basicProperties: null,
                                         body: ms.ToArray());
                    _Logger.LogInformation($"Sent video file for analysis + {ms.ToArray()}");

                    return true;
                }
            }
            catch (Exception e)
            {
                _Logger.LogInformation($"Can't connect to RabbitMQ: {e.Message}");
                return false;
            }

        }


        // Receiving the results from the game analysis
        public Task<string> ReceiveGameAnalysis()
        {
            string receivedMessage = "";

            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "swift_rehab_app", type: "topic");
                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queue: queueName,
                                  exchange: "swift_rehab_app",
                                  routingKey: "game.toC#");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    receivedMessage = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    _Logger.LogInformation("Video analysis received '{0}':'{1}", routingKey, receivedMessage);
                };

                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                int logValue = 0;
                while (receivedMessage == "")
                {
                    logValue++;
                    if (logValue % 500 == 0)
                    {
                        _Logger.LogInformation("Haven't received result yet");
                    }
                    Thread.Sleep(10);
                    channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);
                    if (logValue > 1000)
                    {
                        return Task.FromResult("The request has timed out");
                    }
                }

                return Task.FromResult(receivedMessage);
            }
        }

        public Task<string> ReceiveVideonalysis()
        {
            string receivedMessage = "";

            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "swift_rehab_app", type: "topic");
                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queue: queueName,
                                  exchange: "swift_rehab_app",
                                  routingKey: "video.toC#");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    receivedMessage = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                };

                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                int logValue = 0;
                while (receivedMessage == "")
                {
                    logValue++;
                    if (logValue % 500 == 0)
                    {
                        _Logger.LogInformation("Haven't received result yet");
                    }
                    Thread.Sleep(10);
                    channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);
                    if (logValue > 1000)
                    {
                        return Task.FromResult("The request has timed out");
                    }
                }

                return Task.FromResult(receivedMessage);

            }
        }
    }
}

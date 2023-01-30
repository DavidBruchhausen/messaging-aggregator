using System.Text;
using MessagingAggregator.Application.Interfaces;
using MessagingAggregator.Application.Models;
using RabbitMQ.Client;
using MessagingAggregator.Application.Common.Extensions;
using Serilog;
using Microsoft.Extensions.Configuration;

namespace MessagingAggregator.Application.Services;

public class QueueService : IQueueService
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger _logger = Log.ForContext<QueueService>();
    private static readonly int _maxRetries = 3;
    private static readonly int _retryInterval = 1000; // milliseconds

    public QueueService(IConfiguration configuration)
    {
        _factory = new ConnectionFactory
        {
            HostName = configuration.GetValue<string>("RABBITMQ_HOSTNAME"),
            UserName = configuration.GetValue<string>("RABBITMQ_USERNAME"),
            Password = configuration.GetValue<string>("RABBITMQ_PASSWORD"),
            Port = 5672
        };
    }

    public void SendMessage(Message model)
    {
        int retries = 0;
        bool success = false;

        while (!success && retries < _maxRetries)
        {
            try
            {
                using var connection = _factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: "messages", durable: true, exclusive: false, autoDelete: false);

                var jsonMessage = model.AsJson();
                var body = Encoding.UTF8.GetBytes(jsonMessage);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish("", "messages", properties, body);

                _logger.Information("Published message: \n {0}", jsonMessage);
                success = true;
            }

            catch (Exception ex)
            {
                retries++;
                if (retries < _maxRetries)
                {
                    _logger.Warning("Error occurred while sending message: {0} . Retrying in {1} ms", ex.Message, _retryInterval);
                    Thread.Sleep(_retryInterval);
                }
                else
                {
                    _logger.Error(ex, "Error occurred while sending message: {0} . Maximum number of retries reached", ex.Message);
                }
            }
        }
    }
}
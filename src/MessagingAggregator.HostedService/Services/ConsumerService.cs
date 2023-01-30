using System.Text;
using MessagingAggregator.Application.Common.Extensions;
using MessagingAggregator.Application.Interfaces;
using MessagingAggregator.Application.Models;
using MessagingAggregator.HostedService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace MessagingAggregator.HostedService.Services;

public class ConsumerService : BackgroundService
{
    private readonly ConnectionFactory _factory;
    private readonly IProvider _provider;
    private readonly ILogger _logger = Log.ForContext<ConsumerService>();
    private readonly int _batchSize;
    private readonly int _pollInterval;
    private static readonly int _maxRetries = 3;
    private static readonly int _retryInterval = 1000; // milliseconds
    private static readonly string _queueName = "messages";

    public ConsumerService(IConfiguration configuration, IProvider provider)
    {
        _provider = provider;
        _batchSize = configuration.GetValue<int>("MAXIMUM_BATCH_SIZE");
        _pollInterval = configuration.GetValue<int>("BATCH_POLL_INTERVAL");
        _factory = new ConnectionFactory
        {
            HostName = configuration.GetValue<string>("RABBITMQ_HOSTNAME"),
            UserName = configuration.GetValue<string>("RABBITMQ_USERNAME"),
            Password = configuration.GetValue<string>("RABBITMQ_PASSWORD"),
            Port = 5672
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("ConsumerService is starting.");

        stoppingToken.Register(() => _logger.Information("ConsumerService background task is stopping."));

        _logger.Information("Sleeping to wait for RabbitMQ to start up...");
        Task.Delay(10000).Wait();

        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);

        var messages = new List<Message>();
        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (sender, ea) =>
        {
            string messageJson = Encoding.UTF8.GetString(ea.Body.ToArray());
            Message message = messageJson.FromJson<Message>();
            _logger.Information("Received message: \n {message}", messageJson);

            messages.Add(message);
            channel.BasicAck(ea.DeliveryTag, false);
        };

        channel.BasicConsume(_queueName, false, consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.Information("ConsumerService task doing background work.");
            await Task.Delay(_pollInterval, stoppingToken);

            int numberOfMessagesToSend = Math.Min(messages.Count, _batchSize);
            if (numberOfMessagesToSend > 0)
            {
                List<Batch> batches = GroupMessages(messages.Take(numberOfMessagesToSend).ToList());
                bool success = await SendBatches(batches, stoppingToken);
                if (success)
                {
                    messages.RemoveRange(0, numberOfMessagesToSend);
                }
            }
        }

        _logger.Information("ConsumerService background task is stopping.");
    }

    private async Task<bool> SendBatches(List<Batch> batches, CancellationToken stoppingToken)
    {
        int retries = 0;
        bool success = false;
        while (retries < _maxRetries)
        {
            try
            {
                var request = new
                {
                    Batches = batches
                };
                await _provider.SendPostRequest(request, "aggregated-messages");
                _logger.Information("Sent batch: \n {0}", batches.AsJson());
                success = true;
                break;
            }
            catch (Exception ex)
            {
                retries++;
                if (retries < _maxRetries)
                {
                    _logger.Warning("Error occurred while sending message: {0} . Retrying in {1} ms", ex.Message, _retryInterval);
                    await Task.Delay(_retryInterval, stoppingToken);
                }
                else
                {
                    _logger.Error(ex, "Error occurred while sending message: {0} . Maximum number of retries reached", ex.Message);
                    break;
                }
            }
        }

        return success;
    }

    private List<Batch> GroupMessages(List<Message> messages)
    {
        var batches = new List<Batch>();
        var groupedMessages = messages.GroupBy(m => m.Destination);

        foreach (var group in groupedMessages)
        {
            batches.Add(new Batch
            {
                Destination = group.Key,
                Messages = group.Select(x => new MessageDto
                {
                    Text = x.Text,
                    Timestamp = x.Timestamp
                }).ToList()
            });
        }

        return batches;
    }
}
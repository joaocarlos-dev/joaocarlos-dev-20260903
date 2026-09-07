using System.Text;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Infrastructure.Services.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EmployeeManagmentSystem.API.Configurations.Outbox;

public sealed class RabbitMqOutboxPublisher(
    IServiceScopeFactory scopeFactory,
    RabbitMqOptions options,
    ILogger<RabbitMqOutboxPublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var connection = CreateConnection();
                using var channel = ConfigureChannel(connection);
                while (connection.IsOpen && !stoppingToken.IsCancellationRequested)
                {
                    await PublishPendingAsync(channel, stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(options.PollingSeconds), stoppingToken);
                }
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(exception, "RabbitMQ outbox publishing failed.");
                await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, options.PollingSeconds)), stoppingToken);
            }
        }
    }

    private IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password
        };
        return factory.CreateConnection();
    }

    private IModel ConfigureChannel(IConnection connection)
    {
        var channel = connection.CreateModel();
        channel.ConfirmSelect();
        channel.ExchangeDeclare(options.UnroutedExchange, ExchangeType.Fanout, durable: true, autoDelete: false);
        var quarantineArguments = new Dictionary<string, object>
        {
            ["x-message-ttl"] = checked(options.UnroutedMessageTtlHours * 60 * 60 * 1000),
            ["x-max-length"] = options.UnroutedQueueMaxLength
        };
        channel.QueueDeclare(options.UnroutedQueue, durable: true, exclusive: false, autoDelete: false, arguments: quarantineArguments);
        channel.QueueBind(options.UnroutedQueue, options.UnroutedExchange, string.Empty);
        var exchangeArguments = new Dictionary<string, object>
        {
            ["alternate-exchange"] = options.UnroutedExchange
        };
        channel.ExchangeDeclare(options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false, arguments: exchangeArguments);
        channel.QueueDeclare(options.Queue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(options.Queue, options.Exchange, nameof(Application.Common.Events.UserRegisteredEvent));
        return channel;
    }

    private async Task PublishPendingAsync(IModel channel, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var messages = await outbox.GetPendingAsync(100, cancellationToken);
        await outbox.PruneProcessedAsync(TimeSpan.FromDays(options.ProcessedRetentionDays), 100, cancellationToken);
        if (messages.Count == 0)
        {
            return;
        }

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        foreach (var message in messages)
        {
            var messageId = message.Id.ToString();
            try
            {
                properties.MessageId = messageId;
                channel.BasicPublish(options.Exchange, message.Type, false, properties, Encoding.UTF8.GetBytes(message.Payload));
                channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(10));
                var markedProcessed = await outbox.MarkProcessedAsync(message.Id, outbox.WorkerId, cancellationToken);
                if (!markedProcessed)
                {
                    logger.LogWarning("Outbox message {MessageId} was no longer owned by worker {WorkerId}.", message.Id, outbox.WorkerId);
                }
            }
            catch (Exception exception)
            {
                var markedFailed = await outbox.MarkFailedAsync(message.Id, exception.Message, outbox.WorkerId, cancellationToken);
                if (!markedFailed)
                {
                    logger.LogWarning("Failed outbox message {MessageId} was no longer owned by worker {WorkerId}.", message.Id, outbox.WorkerId);
                }
                logger.LogError(exception, "Failed to publish outbox message {MessageId}.", message.Id);
            }
        }
    }
}

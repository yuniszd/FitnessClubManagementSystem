using System.Text;
using System.Text.Json;
using FCMS.Infrastructure.Messaging;
using RabbitMQ.Client;
using Microsoft.Extensions.Logging;

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqPublisher>? _logger;
    private bool _disposed;

    public RabbitMqPublisher(string hostName = "localhost", ILogger<RabbitMqPublisher>? logger = null)
    {
        _logger = logger;
        var factory = new ConnectionFactory
        {
            HostName = hostName,
            DispatchConsumersAsync = true
        };
        _connection = factory.CreateConnection();
    }

    public Task PublishAsync(string routingKey, string message)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RabbitMqPublisher));

        using var channel = _connection.CreateModel();
        channel.QueueDeclare(routingKey, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var body = Encoding.UTF8.GetBytes(message);
        var props = channel.CreateBasicProperties();
        props.Persistent = true;

        channel.BasicPublish(exchange: "", routingKey: routingKey, basicProperties: props, body: body);
        _logger?.LogDebug("Published message to {Queue}: {Length} bytes", routingKey, body.Length);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _connection?.Close();
        _connection?.Dispose();
        _disposed = true;
    }
}

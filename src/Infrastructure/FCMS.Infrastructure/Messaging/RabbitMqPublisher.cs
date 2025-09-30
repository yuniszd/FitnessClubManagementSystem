using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FCMS.Infrastructure.Messaging;

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly RabbitMqChannelPool _channelPool;
    private readonly ILogger<RabbitMqPublisher>? _logger;
    private bool _disposed;

    public RabbitMqPublisher(RabbitMqChannelPool channelPool, ILogger<RabbitMqPublisher>? logger = null)
    {
        _channelPool = channelPool;
        _logger = logger;
    }

    public Task PublishAsync(string routingKey, string message)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RabbitMqPublisher));

        var channel = _channelPool.RentChannel();

        try
        {
            channel.QueueDeclare(routingKey, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var body = Encoding.UTF8.GetBytes(message);
            var props = channel.CreateBasicProperties();
            props.Persistent = true;

            channel.BasicPublish(exchange: "", routingKey: routingKey, basicProperties: props, body: body);

            _logger?.LogDebug("Published message to {Queue}: {Length} bytes", routingKey, body.Length);
        }
        finally
        {
            _channelPool.ReturnChannel(channel);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _disposed = true;
    }
}

using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace FCMS.Infrastructure.Messaging;

public class RabbitMqChannelPool : IDisposable
{
    private readonly IConnection _connection;
    private readonly ConcurrentBag<IModel> _channels = new();
    private readonly int _maxChannels;
    private bool _disposed;

    public RabbitMqChannelPool(IConnection connection, int maxChannels = 5)
    {
        _connection = connection;
        _maxChannels = maxChannels;

        for (int i = 0; i < _maxChannels; i++)
        {
            var channel = _connection.CreateModel();
            _channels.Add(channel);
        }
    }

    public IModel RentChannel()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RabbitMqChannelPool));

        if (_channels.TryTake(out var channel) && channel.IsOpen)
            return channel;

        return _connection.CreateModel();
    }

    public void ReturnChannel(IModel channel)
    {
        if (_disposed)
        {
            channel?.Dispose();
            return;
        }

        if (channel.IsOpen)
            _channels.Add(channel);
        else
            channel?.Dispose();
    }

    public void Dispose()
    {
        if (_disposed) return;

        while (_channels.TryTake(out var channel))
        {
            channel?.Close();
            channel?.Dispose();
        }

        _connection?.Close();
        _disposed = true;
    }
}

namespace FCMS.Infrastructure.Messaging;

public interface IRabbitMqPublisher
{
    Task PublishAsync(string routingKey, string message);
}
using System.Text;
using System.Text.Json;
using FCMS.Application.Abstracts;
using FCMS.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FCMS.Infrastructure.Messaging;

public class RabbitMqConsumer<T> : BackgroundService
{
    private readonly RabbitMqChannelPool _channelPool;
    private readonly string _queueName;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMqConsumer<T>> _logger;

    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true, 
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public RabbitMqConsumer(
        RabbitMqChannelPool channelPool,
        string queueName,
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitMqConsumer<T>> logger)
    {
        _channelPool = channelPool;
        _queueName = queueName;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _channelPool.RentChannel();

        channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.Received += async (sender, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                // ✅ Case-insensitive deserialization
                var message = JsonSerializer.Deserialize<T>(json, _jsonOptions);

                if (message == null)
                {
                    _logger.LogWarning("Received null or unparseable message: {Json}", json);
                    channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                switch (message)
                {
                    case CustomerRegisteredEvent evt:
                        await HandleCustomerRegisteredEventAsync(evt, emailService);
                        break;

                    default:
                        _logger.LogWarning("Unknown message type: {Type}", typeof(T).Name);
                        break;
                }

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RabbitMQ message for queue {Queue}", _queueName);
                channel.BasicNack(ea.DeliveryTag, false, requeue: false); // ❌ Flood problemi olmayacaq
            }
        };

        channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    private async Task HandleCustomerRegisteredEventAsync(CustomerRegisteredEvent evt, IEmailService emailService)
    {
        if (string.IsNullOrWhiteSpace(evt.Email))
        {
            _logger.LogWarning("Email boşdur, mesaj göndərilməyəcək: {FullName}", evt.FullName);
            return;
        }

        var subject = "Gym-ə xoş gəlmisiniz!";
        var body = $@"
            Salam {evt.FullName},<br/><br/>
            Sizin abonementiniz '{evt.PlanName}' planı ilə aktivdir. 
            Bitmə tarixi: {evt.SubscriptionEndDate:dd/MM/yyyy}.<br/><br/>
            Hər ziyarətinizdə QR kodu təqdim edin.<br/><br/>
            Sağlamlıqla qalın!<br/>
            Gym Management";

        try
        {
            await emailService.SendEmailAsync(evt.Email, subject, body, evt.QrCodeAttachment);
            _logger.LogInformation("📧 Email göndərildi: {Email}", evt.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email göndərilərkən xəta baş verdi: {Email}", evt.Email);
        }
    }
}

using System.Text;
using System.Text.Json;
using FCMS.Application.Abstracts;
using FCMS.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FCMS.Infrastructure.Messaging
{
    public class RabbitMqConsumer<T> : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly ILogger<RabbitMqConsumer<T>> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMqConsumer(
            string hostName,
            string queueName,
            IServiceScopeFactory scopeFactory,
            ILogger<RabbitMqConsumer<T>> logger)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _queueName = queueName;

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (sender, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<T>(json);

                    if (message is null)
                    {
                        _logger.LogWarning("⚠️ Boş və ya deserialize olunmayan mesaj gəldi.");
                        _channel.BasicAck(ea.DeliveryTag, false);
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
                            _logger.LogWarning("⚠️ Tanınmayan mesaj tipi: {Type}", typeof(T).Name);
                            break;
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ RabbitMQ mesajı işlənərkən xəta baş verdi.");
                    _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
                }
            };

            _channel.BasicConsume(_queueName, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        private async Task HandleCustomerRegisteredEventAsync(CustomerRegisteredEvent evt, IEmailService emailService)
        {
            var subject = "Gym-ə xoş gəlmisiniz!";
            var body = $@"
                Salam {evt.FullName},<br/><br/>
                Sizin abonementiniz '{evt.PlanName}' planı ilə aktivdir. 
                Bitmə tarixi: {evt.SubscriptionEndDate:dd/MM/yyyy}.<br/><br/>
                Hər ziyarətinizdə QR kodu təqdim edin.<br/><br/>
                Sağlamlıqla qalın!<br/>
                Gym Management";

            await emailService.SendEmailAsync(evt.Email, subject, body, evt.QrCodeAttachment);

            _logger.LogInformation("📧 Email göndərildi: {Email}", evt.Email);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}

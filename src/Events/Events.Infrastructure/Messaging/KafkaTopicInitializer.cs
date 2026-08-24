using BookingContracts;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Events.Infrastructure.Messaging
{
    public class KafkaTopicInitializer : BackgroundService
    {
        private readonly KafkaOptions _options;
        private readonly ILogger<KafkaTopicInitializer> _logger;

        public KafkaTopicInitializer(KafkaOptions options, ILogger<KafkaTopicInitializer> logger)
        {
            _options = options;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await EnsureTopicExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Не удалось создать топик Kafka. Продолжаем запуск.");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

        private async Task EnsureTopicExistsAsync()
        {
            using var adminClient = new AdminClientBuilder(
                new AdminClientConfig { BootstrapServers = _options.BootstrapServers }).Build();

            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            if (metadata.Topics.Any(t => t.Topic == Topics.BookingConfirmed))
                return;

            await adminClient.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = Topics.BookingConfirmed,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
            });

            _logger.LogInformation("Топик {Topic} создан.", Topics.BookingConfirmed);
        }
    }
}

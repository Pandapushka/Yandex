using System.Text.Json;
using BookingContracts;
using Bookings.Application.Interfaces;
using Confluent.Kafka;

namespace Bookings.Infrastructure.Messaging
{
    public class KafkaEventPublisher : IEventPublisher, IDisposable
    {
        private readonly IProducer<string, string> _producer;

        public KafkaEventPublisher(KafkaOptions options)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = options.BootstrapServers,
                Acks = Acks.All
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishAsync(BookingConfirmed bookingConfirmed, CancellationToken cancellationToken = default)
        {
            var message = new Message<string, string>
            {
                Key = bookingConfirmed.EventId.ToString(),
                Value = JsonSerializer.Serialize(bookingConfirmed)
            };

            await _producer.ProduceAsync(Topics.BookingConfirmed, message, cancellationToken);
        }

        public void Dispose() => _producer.Dispose();
    }
}

namespace Events.Infrastructure.Messaging
{
    public class KafkaOptions
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string ConsumerGroup { get; set; } = "events-booking-consumer";
    }
}

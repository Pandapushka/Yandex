using System.Text.Json;
using BookingContracts;
using Confluent.Kafka;
using Events.Application.Interfaces;
using Events.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Events.Infrastructure.Messaging
{
    public class BookingConfirmedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly KafkaOptions _options;
        private readonly ILogger<BookingConfirmedConsumer> _logger;

        public BookingConfirmedConsumer(
            IServiceScopeFactory scopeFactory,
            KafkaOptions options,
            ILogger<BookingConfirmedConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => Consume(stoppingToken), stoppingToken);
        }

        private void Consume(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                GroupId = _options.ConsumerGroup,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(Topics.BookingConfirmed);

            _logger.LogInformation("Подписчик запущен. Ожидание сообщений из топика '{Topic}'...", Topics.BookingConfirmed);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    try
                    {
                        ProcessMessageAsync(consumeResult.Message.Value, stoppingToken).GetAwaiter().GetResult();
                    }
                    catch (NotFoundException ex)
                    {
                        _logger.LogWarning(ex, "Событие не найдено. Сообщение пропущено.");
                    }
                    catch (NoAvailableSeatsException ex)
                    {
                        _logger.LogWarning(ex, "Нет свободных мест. Сообщение пропущено.");
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Не удалось десериализовать сообщение. Сообщение пропущено.");
                    }

                    consumer.Commit(consumeResult);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Подписчик остановлен штатно.");
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Ошибка при получении сообщения.");
            }
            finally
            {
                consumer.Close();
            }
        }

        private async Task ProcessMessageAsync(string? value, CancellationToken stoppingToken)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new JsonException("Пустое сообщение");

            var message = JsonSerializer.Deserialize<BookingConfirmed>(value);
            if (message == null)
                throw new JsonException("Невозможно десериализовать сообщение");

            using var scope = _scopeFactory.CreateScope();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

            await eventService.DecreaseAvailableSeatsAsync(message.EventId, message.Seats, stoppingToken);

            _logger.LogInformation(
                "Обработано BookingConfirmed: BookingId={BookingId}, EventId={EventId}, Seats={Seats}",
                message.BookingId, message.EventId, message.Seats);
        }
    }
}

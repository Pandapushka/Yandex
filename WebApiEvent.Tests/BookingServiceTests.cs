using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApiEvent.Application.DTOs.Event;
using WebApiEvent.Application.Interfaces;
using WebApiEvent.Application.Services;
using WebApiEvent.Domain.Enums;
using WebApiEvent.Domain.Exceptions;
using WebApiEvent.Infrastructure.Persistence;
using WebApiEvent.Infrastructure.Repositories;

namespace WebApiEvent.Tests
{
    public class BookingServiceTests
    {
        private readonly IServiceProvider _serviceProvider;

        public BookingServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();
            _serviceProvider = services.BuildServiceProvider();
        }

        private static async Task<Guid> CreateFutureEventAsync(IEventService eventService)
            => await eventService.CreateAsync(new EventDtoRequest(
                "Event", "Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2)));

        [Fact]
        public async Task CreateBookingAsync_ValidEvent_ReturnsPendingBooking()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventId = await CreateFutureEventAsync(eventService);

            var result = await bookingService.CreateBookingAsync(Guid.NewGuid(), eventId);

            result.Should().NotBeNull();
            result.EventId.Should().Be(eventId);
            result.Status.Should().Be(BookingStatus.Pending);
            result.ProcessedAt.Should().BeNull();
        }

        [Fact]
        public async Task GetBookingAsync_ExistingBooking_ReturnsCorrectData()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventId = await CreateFutureEventAsync(eventService);
            var userId = Guid.NewGuid();
            var booking = await bookingService.CreateBookingAsync(userId, eventId);

            var result = await bookingService.GetBookingAsync(booking.Id, userId, isAdmin: false);

            result.Id.Should().Be(booking.Id);
            result.EventId.Should().Be(eventId);
            result.Status.Should().Be(BookingStatus.Pending);
        }

        [Fact]
        public async Task CreateBookingAsync_MultipleBookings_AllHaveUniqueIds()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventId = await CreateFutureEventAsync(eventService);
            var userId = Guid.NewGuid();

            var first = await bookingService.CreateBookingAsync(userId, eventId);
            var second = await bookingService.CreateBookingAsync(userId, eventId);

            first.Id.Should().NotBe(second.Id);
        }

        [Fact]
        public async Task CreateBookingAsync_NonExistingEvent_ThrowsNotFoundException()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            Func<Task> act = async () => await bookingService.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid());
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CreateBookingAsync_SoftDeletedEvent_ThrowsNotFoundException()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventId = await CreateFutureEventAsync(eventService);
            await eventService.SoftDeleteAsync(eventId);

            Func<Task> act = async () => await bookingService.CreateBookingAsync(Guid.NewGuid(), eventId);
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetBookingAsync_NonExistingId_ThrowsNotFoundException()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            Func<Task> act = async () => await bookingService.GetBookingAsync(Guid.NewGuid(), Guid.NewGuid(), isAdmin: false);
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetBookingAsync_OtherUser_ThrowsForbiddenException()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventId = await CreateFutureEventAsync(eventService);
            var owner = Guid.NewGuid();
            var booking = await bookingService.CreateBookingAsync(owner, eventId);

            Func<Task> act = async () =>
                await bookingService.GetBookingAsync(booking.Id, Guid.NewGuid(), isAdmin: false);
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task GetBookingAsync_Admin_CanViewAnyBooking()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventId = await CreateFutureEventAsync(eventService);
            var booking = await bookingService.CreateBookingAsync(Guid.NewGuid(), eventId);

            var result = await bookingService.GetBookingAsync(booking.Id, Guid.NewGuid(), isAdmin: true);

            result.Id.Should().Be(booking.Id);
        }

        [Fact]
        public async Task CreateBookingAsync_AlreadyStartedEvent_ThrowsEventAlreadyStartedException()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

            // Событие началось в прошлом, но ещё не завершилось.
            var eventId = await eventService.CreateAsync(new EventDtoRequest(
                "Past", "Description", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(1)));

            Func<Task> act = async () => await bookingService.CreateBookingAsync(Guid.NewGuid(), eventId);
            await act.Should().ThrowAsync<EventAlreadyStartedException>();
        }

        [Fact]
        public async Task CreateBookingAsync_WhenLimitReached_ThrowsBookingLimitExceededException()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventId = await CreateFutureEventAsync(eventService);
            var userId = Guid.NewGuid();

            for (var i = 0; i < 10; i++)
                await bookingService.CreateBookingAsync(userId, eventId);

            Func<Task> act = async () => await bookingService.CreateBookingAsync(userId, eventId);
            await act.Should().ThrowAsync<BookingLimitExceededException>()
                .WithMessage("*10*");
        }

        [Fact]
        public async Task CreateBookingAsync_LimitsArePerUser()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventId = await CreateFutureEventAsync(eventService);

            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();

            for (var i = 0; i < 10; i++)
                await bookingService.CreateBookingAsync(userA, eventId);

            // Лимит пользователя B не зависит от пользователя A.
            var result = await bookingService.CreateBookingAsync(userB, eventId);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task CancelBookingAsync_Owner_CanCancel()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventId = await CreateFutureEventAsync(eventService);
            var userId = Guid.NewGuid();
            var booking = await bookingService.CreateBookingAsync(userId, eventId);

            await bookingService.CancelBookingAsync(booking.Id, userId, isAdmin: false);

            (await bookingService.GetBookingAsync(booking.Id, userId, isAdmin: false)).Status.Should().Be(BookingStatus.Cancelled);
        }

        [Fact]
        public async Task CancelBookingAsync_OtherUser_ThrowsForbiddenException()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventId = await CreateFutureEventAsync(eventService);
            var owner = Guid.NewGuid();
            var booking = await bookingService.CreateBookingAsync(owner, eventId);

            Func<Task> act = async () =>
                await bookingService.CancelBookingAsync(booking.Id, Guid.NewGuid(), isAdmin: false);
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task CancelBookingAsync_Admin_CanCancelAnyBooking()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventId = await CreateFutureEventAsync(eventService);
            var booking = await bookingService.CreateBookingAsync(Guid.NewGuid(), eventId);

            await bookingService.CancelBookingAsync(booking.Id, Guid.NewGuid(), isAdmin: true);

            (await bookingService.GetBookingAsync(booking.Id, Guid.NewGuid(), isAdmin: true)).Status.Should().Be(BookingStatus.Cancelled);
        }

        [Fact]
        public async Task CancelBookingAsync_AfterEventStart_ThrowsEventAlreadyStartedException()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventId = await CreateFutureEventAsync(eventService);
            var userId = Guid.NewGuid();
            var booking = await bookingService.CreateBookingAsync(userId, eventId);

            // Сдвигаем событие в прошлое, чтобы смоделировать "уже началось".
            await eventService.UpdateAsync(eventId, new UpdateEventDtoRequest(
                StartAt: DateTime.UtcNow.AddHours(-1),
                EndAt: DateTime.UtcNow.AddHours(1)));

            Func<Task> act = async () =>
                await bookingService.CancelBookingAsync(booking.Id, userId, isAdmin: false);
            await act.Should().ThrowAsync<EventAlreadyStartedException>();
        }
    }
}

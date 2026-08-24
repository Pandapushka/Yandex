using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Bookings.Application.Interfaces;
using Bookings.Application.Services;
using Bookings.Domain.Enums;
using Bookings.Domain.Exceptions;
using Bookings.Infrastructure.Persistence;
using Bookings.Infrastructure.Repositories;

namespace Bookings.Tests
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
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingService, BookingService>();
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task CreateBookingAsync_ReturnsPendingBooking()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var result = await bookingService.CreateBookingAsync(userId, eventId);

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
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();
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
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var first = await bookingService.CreateBookingAsync(userId, eventId);
            var second = await bookingService.CreateBookingAsync(userId, eventId);

            first.Id.Should().NotBe(second.Id);
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
            var owner = Guid.NewGuid();
            var booking = await bookingService.CreateBookingAsync(owner, Guid.NewGuid());

            Func<Task> act = async () =>
                await bookingService.GetBookingAsync(booking.Id, Guid.NewGuid(), isAdmin: false);
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task GetBookingAsync_Admin_CanViewAnyBooking()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var booking = await bookingService.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid());

            var result = await bookingService.GetBookingAsync(booking.Id, Guid.NewGuid(), isAdmin: true);

            result.Id.Should().Be(booking.Id);
        }

        [Fact]
        public async Task CreateBookingAsync_WhenLimitReached_ThrowsBookingLimitExceededException()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var userId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

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
            var eventId = Guid.NewGuid();

            var userA = Guid.NewGuid();
            var userB = Guid.NewGuid();

            for (var i = 0; i < 10; i++)
                await bookingService.CreateBookingAsync(userA, eventId);

            var result = await bookingService.CreateBookingAsync(userB, eventId);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task CancelBookingAsync_Owner_CanCancel()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var userId = Guid.NewGuid();
            var booking = await bookingService.CreateBookingAsync(userId, Guid.NewGuid());

            await bookingService.CancelBookingAsync(booking.Id, userId, isAdmin: false);

            (await bookingService.GetBookingAsync(booking.Id, userId, isAdmin: false)).Status.Should().Be(BookingStatus.Cancelled);
        }

        [Fact]
        public async Task CancelBookingAsync_OtherUser_ThrowsForbiddenException()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var owner = Guid.NewGuid();
            var booking = await bookingService.CreateBookingAsync(owner, Guid.NewGuid());

            Func<Task> act = async () =>
                await bookingService.CancelBookingAsync(booking.Id, Guid.NewGuid(), isAdmin: false);
            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task CancelBookingAsync_Admin_CanCancelAnyBooking()
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var booking = await bookingService.CreateBookingAsync(Guid.NewGuid(), Guid.NewGuid());

            await bookingService.CancelBookingAsync(booking.Id, Guid.NewGuid(), isAdmin: true);

            (await bookingService.GetBookingAsync(booking.Id, Guid.NewGuid(), isAdmin: true)).Status.Should().Be(BookingStatus.Cancelled);
        }
    }
}

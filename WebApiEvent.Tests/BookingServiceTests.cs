using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApiEvent.CustomExceptions;
using WebApiEvent.DataAccess;
using WebApiEvent.DataAccess.Repositories;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Models.Enums;
using WebApiEvent.Services;

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

    [Fact]
    public async Task CreateBookingAsync_ValidEventId_ReturnsBookingResponseWithPendingStatus()
    {
        using var scope = _serviceProvider.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var eventId = await eventService.CreateAsync(new EventDtoRequest("Event", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1)));

        var result = await bookingService.CreateBookingAsync(eventId);

        result.Should().NotBeNull();
        result.EventId.Should().Be(eventId);
        result.Status.Should().Be(BookingStatus.Pending);
        result.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task GetBookingAsync_ExistingBooking_ReturnsCorrectBookingResponse()
    {
        using var scope = _serviceProvider.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var eventId = await eventService.CreateAsync(new EventDtoRequest("Event", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1)));
        var booking = await bookingService.CreateBookingAsync(eventId);

        var result = await bookingService.GetBookingAsync(booking.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(booking.Id);
        result.EventId.Should().Be(eventId);
        result.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task CreateBookingAsync_MultipleBookingsForSameEvent_AllHaveUniqueIds()
    {
        using var scope = _serviceProvider.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var eventId = await eventService.CreateAsync(new EventDtoRequest("Event", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1)));

        var booking1 = await bookingService.CreateBookingAsync(eventId);
        var booking2 = await bookingService.CreateBookingAsync(eventId);

        booking1.Id.Should().NotBe(booking2.Id);
    }

    [Fact]
    public async Task CreateBookingAsync_NonExistingEvent_ThrowsNotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventId = Guid.NewGuid();

        Func<Task> act = async () => await bookingService.CreateBookingAsync(eventId);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*не найдено*");
    }

    [Fact]
    public async Task CreateBookingAsync_SoftDeletedEvent_ThrowsNotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var eventId = await eventService.CreateAsync(new EventDtoRequest("Event", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1)));
        await eventService.SoftDeleteAsync(eventId);

        Func<Task> act = async () => await bookingService.CreateBookingAsync(eventId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetBookingAsync_NonExistingId_ThrowsNotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var nonExistentId = Guid.NewGuid();

        Func<Task> act = async () => await bookingService.GetBookingAsync(nonExistentId);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*не найдена*");
    }

    [Fact]
    public async Task CreateBookingAsync_WithEmptyEventId_ThrowsNotFoundException()
    {
        using var scope = _serviceProvider.CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var emptyEventId = Guid.Empty;

        Func<Task> act = async () => await bookingService.CreateBookingAsync(emptyEventId);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
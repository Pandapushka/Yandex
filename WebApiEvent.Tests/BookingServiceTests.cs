using FluentAssertions;
using Moq;
using WebApiEvent.CustomExceptions;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Models.Entity;
using WebApiEvent.Models.Enums;
using WebApiEvent.Services;

public class BookingServiceTests
{
    [Fact]
    public async Task CreateBookingAsync_ValidEventId_ReturnsBookingResponseWithPendingStatusAndDecreasesSeats()
    {
        var events = new List<Event> { Event.Create("Event", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 5) };
        var bookings = new List<Booking>();
        var service = new BookingService(bookings, events);
        var eventId = events[0].Id;

        var result = await service.CreateBookingAsync(eventId);

        result.Should().NotBeNull();
        result.EventId.Should().Be(eventId);
        result.Status.Should().Be(BookingStatus.Pending);
        events[0].AvailableSeats.Should().Be(4);
        bookings.Should().ContainSingle(b => b.Id == result.Id);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenNoSeats_ThrowsNoAvailableSeatsException()
    {
        var events = new List<Event> { Event.Create("Event", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 1) };
        var bookings = new List<Booking>();
        var service = new BookingService(bookings, events);

        await service.CreateBookingAsync(events[0].Id);

        Func<Task> act = async () => await service.CreateBookingAsync(events[0].Id);
        await act.Should().ThrowAsync<NoAvailableSeatsException>().WithMessage("*свободных мест*");
        events[0].AvailableSeats.Should().Be(0);
    }

    [Fact]
    public async Task CreateBookingAsync_NonExistingEvent_ThrowsNotFoundException()
    {
        var events = new List<Event>();
        var bookings = new List<Booking>();
        var service = new BookingService(bookings, events);

        Func<Task> act = async () => await service.CreateBookingAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetBookingAsync_ExistingBooking_ReturnsCorrectBooking()
    {
        var events = new List<Event>();
        var bookings = new List<Booking>();
        var service = new BookingService(bookings, events);
        var eventId = Guid.NewGuid();
        var booking = Booking.CreatePending(eventId);
        bookings.Add(booking);

        var result = await service.GetBookingAsync(booking.Id);

        result.Id.Should().Be(booking.Id);
        result.EventId.Should().Be(eventId);
    }

    [Fact]
    public async Task GetBookingAsync_NonExistingId_ThrowsNotFoundException()
    {
        var service = new BookingService(new List<Booking>(), new List<Event>());

        Func<Task> act = async () => await service.GetBookingAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateBookingAsync_ConcurrentOverbooking_OnlyAllowedNumberOfBookingsSucceed()
    {
        var events = new List<Event> { Event.Create("Concert", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 5) };
        var bookings = new List<Booking>();
        var service = new BookingService(bookings, events);
        var eventId = events[0].Id;
        int concurrentRequests = 20;

        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => Task.Run(() => service.CreateBookingAsync(eventId)));

        var results = await Task.WhenAll(tasks.Select(t => t.ContinueWith(tr =>
            (Success: !tr.IsFaulted, Exception: tr.Exception?.InnerException))));

        int successCount = results.Count(r => r.Success);
        int failureCount = results.Count(r => !r.Success && r.Exception is NoAvailableSeatsException);
        successCount.Should().Be(5);
        failureCount.Should().Be(15);
        events[0].AvailableSeats.Should().Be(0);
    }

    [Fact]
    public async Task CreateBookingAsync_ConcurrentRequests_AllBookingsHaveUniqueIds()
    {
        var events = new List<Event> { Event.Create("Concert", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 10) };
        var bookings = new List<Booking>();
        var service = new BookingService(bookings, events);
        var eventId = events[0].Id;
        int concurrentRequests = 10;

        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => Task.Run(() => service.CreateBookingAsync(eventId)));

        var results = await Task.WhenAll(tasks);

        var ids = results.Select(r => r.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
        bookings.Should().HaveCount(10);
    }

    [Fact]
    public void Booking_Confirm_SetsStatusConfirmedAndProcessedAt()
    {
        var booking = Booking.CreatePending(Guid.NewGuid());
        booking.Confirm();

        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Booking_Reject_SetsStatusRejectedAndProcessedAt()
    {
        var booking = Booking.CreatePending(Guid.NewGuid());
        booking.Reject();

        booking.Status.Should().Be(BookingStatus.Rejected);
        booking.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public void Event_ReleaseSeats_RestoresAvailableSeatsAndAllowsNewBooking()
    {
        var eventEntity = Event.Create("Test", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 3);
        eventEntity.TryReserveSeats(2);
        eventEntity.AvailableSeats.Should().Be(1);

        eventEntity.ReleaseSeats(1);
        eventEntity.AvailableSeats.Should().Be(2);
    }
}
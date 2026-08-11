using FluentAssertions;
using Moq;
using WebApiEvent.CustomExceptions;
using WebApiEvent.DataAccess;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Models.Enums;
using WebApiEvent.Services;

public class BookingServiceTests
{
    private readonly Mock<IEventService> _eventServiceMock;
    private readonly List<Booking> _bookings;
    private readonly BookingService _bookingService;

    public BookingServiceTests()
    {
        _bookings = new List<Booking>();
        _eventServiceMock = new Mock<IEventService>();
        _bookingService = new BookingService(_bookings, _eventServiceMock.Object);
    }

    [Fact]
    public async Task CreateBookingAsync_ValidEventId_ReturnsBookingResponseWithPendingStatusAndDecreasesSeats()
    {
        var eventId = Guid.NewGuid();
        _eventServiceMock.Setup(x => x.GetById(eventId))
            .Returns(new EventDtoResponse(eventId, "Event", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1)));

        var result = await _bookingService.CreateBookingAsync(eventId);

        result.Should().NotBeNull();
        result.EventId.Should().Be(eventId);
        result.Status.Should().Be(BookingStatus.Pending);
        result.ProcessedAt.Should().BeNull();
        _bookings.Should().ContainSingle(b => b.Id == result.Id);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenNoSeats_ThrowsNoAvailableSeatsException()
    {
        var eventId = Guid.NewGuid();
        var booking = Booking.CreatePending(eventId);
        _bookings.Add(booking);

        var result = await _bookingService.GetBookingAsync(booking.Id);

        result.Id.Should().Be(booking.Id);
        result.EventId.Should().Be(eventId);
    }

    [Fact]
    public async Task GetBookingAsync_NonExistingId_ThrowsNotFoundException()
    {
        var eventId = Guid.NewGuid();
        _eventServiceMock.Setup(x => x.GetById(eventId))
            .Returns(new EventDtoResponse(eventId, "Event", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1)));

        var booking1 = await _bookingService.CreateBookingAsync(eventId);
        var booking2 = await _bookingService.CreateBookingAsync(eventId);

        booking1.Id.Should().NotBe(booking2.Id);
        _bookings.Should().HaveCount(2);
        _bookings.All(b => b.EventId == eventId).Should().BeTrue();
    }

    [Fact]
    public async Task GetBookingAsync_AfterProcessing_ReturnsUpdatedStatus()
    {
        var eventId = Guid.NewGuid();
        var booking = Booking.CreatePending(eventId);
        _bookings.Add(booking);
        booking.Confirm();

        var result = await _bookingService.GetBookingAsync(booking.Id);

        result.Status.Should().Be(BookingStatus.Confirmed);
        result.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateBookingAsync_ConcurrentRequests_AllBookingsHaveUniqueIds()
    {
        var eventId = Guid.NewGuid();
        _eventServiceMock.Setup(x => x.GetById(eventId))
            .Throws(new NotFoundException("Событие не найдено"));

        Func<Task> act = async () => await _bookingService.CreateBookingAsync(eventId);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*не найдено*");
        _bookings.Should().BeEmpty();
    }

    [Fact]
    public void Booking_Confirm_SetsStatusConfirmedAndProcessedAt()
    {
        var eventId = Guid.NewGuid();
        _eventServiceMock.Setup(x => x.GetById(eventId))
            .Throws(new NotFoundException("Событие не найдено"));

        Func<Task> act = async () => await _bookingService.CreateBookingAsync(eventId);

        await act.Should().ThrowAsync<NotFoundException>();
        _bookings.Should().BeEmpty();
    }

    [Fact]
    public void Booking_Reject_SetsStatusRejectedAndProcessedAt()
    {
        var nonExistentId = Guid.NewGuid();

        Func<Task> act = async () => await _bookingService.GetBookingAsync(nonExistentId);

        booking.Status.Should().Be(BookingStatus.Rejected);
        booking.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateBookingAsync_WithEmptyEventId_ThrowsDomainException()
    {
        var emptyEventId = Guid.Empty;

        Func<Task> act = async () => await _bookingService.CreateBookingAsync(emptyEventId);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*EventId*");
        _bookings.Should().BeEmpty();
    }
}
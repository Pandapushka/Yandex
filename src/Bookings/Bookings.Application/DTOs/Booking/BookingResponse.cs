using Bookings.Domain.Enums;

namespace Bookings.Application.DTOs.Booking
{
    public record BookingResponse(
        Guid Id,
        Guid EventId,
        BookingStatus Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt
    );
}

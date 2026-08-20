using WebApiEvent.Domain.Enums;

namespace WebApiEvent.Application.DTOs.Booking
{
    public record BookingResponse(
        Guid Id,
        Guid EventId,
        BookingStatus Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt
    );
}

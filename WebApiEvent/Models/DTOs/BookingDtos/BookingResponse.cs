using WebApiEvent.Models.Enums;

namespace WebApiEvent.Models.DTOs.BookingDtos
{
    public record BookingResponse(
        Guid Id,
        Guid EventId,
        BookingStatus Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt
    );
}
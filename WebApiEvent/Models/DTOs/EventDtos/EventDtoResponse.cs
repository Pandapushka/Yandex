namespace WebApiEvent.Models.DTOs.EventDtos
{
    public record EventDtoResponse
    (
        Guid Id,
        string Title,
        string? Description,
        DateTime StartAt,
        DateTime EndAt,
        int TotalSeats,
        int AvailableSeats
    );
}
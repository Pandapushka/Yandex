namespace Events.Application.DTOs.Event
{
    public record EventDtoResponse
    (
        Guid Id,
        string Title,
        string? Description,
        DateTime StartAt,
        DateTime EndAt,
        int AvailableSeats
    );
}

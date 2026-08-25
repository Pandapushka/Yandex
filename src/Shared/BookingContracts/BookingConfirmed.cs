namespace BookingContracts
{
    public record BookingConfirmed(
        Guid BookingId,
        Guid EventId,
        Guid UserId,
        int Seats,
        DateTime ConfirmedAt);
}

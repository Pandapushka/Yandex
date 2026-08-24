namespace Bookings.Domain.Exceptions
{
    public class BookingLimitExceededException : Exception
    {
        public BookingLimitExceededException(int limit)
            : base($"Превышен лимит активных броней. Максимум: {limit}") { }
    }
}

using WebApiEvent.CustomExceptions;
using WebApiEvent.Models.Enums;

namespace WebApiEvent.Models.Entity
{
    public class Booking : BaseEntity
    {
        public Guid EventId { get; private set; }
        public BookingStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }

        private Booking() { } 

        public static Booking CreatePending(Guid eventId)
        {
            if (eventId == Guid.Empty)
                throw new DomainException("EventId не может быть пустым");

            return new Booking
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null
            };
        }

        public void Confirm()
        {
            if (Status != BookingStatus.Pending)
                throw new DomainException($"Невозможно подтвердить бронь в статусе {Status}");

            Status = BookingStatus.Confirmed;
            ProcessedAt = DateTime.UtcNow;
        }

        public void Reject()
        {
            if (Status != BookingStatus.Pending)
                throw new DomainException($"Невозможно отклонить бронь в статусе {Status}");

            Status = BookingStatus.Rejected;
            ProcessedAt = DateTime.UtcNow;
        }
    }
}

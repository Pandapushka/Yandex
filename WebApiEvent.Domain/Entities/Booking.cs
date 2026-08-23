using WebApiEvent.Domain.Enums;
using WebApiEvent.Domain.Exceptions;

namespace WebApiEvent.Domain.Entities
{
    public class Booking : BaseEntity
    {
        public Guid EventId { get; private set; }
        public Guid UserId { get; private set; }
        public BookingStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }

        public Event Event { get; private set; } = null!;
        public User User { get; private set; } = null!;

        private Booking() { }

        public static Booking CreatePending(Guid userId, Guid eventId)
        {
            if(userId == Guid.Empty)
                throw new DomainException("UserId не может быть пустым");
            if (eventId == Guid.Empty)
                throw new DomainException("EventId не может быть пустым");

            return new Booking
            {
                Id = Guid.NewGuid(),
                UserId = userId,
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

        public void Cancel()
        {
            if (Status == BookingStatus.Cancelled)
                throw new DomainException("Бронь уже отменена");
            
            Status = BookingStatus.Cancelled;
            ProcessedAt = DateTime.UtcNow;
        }
    }
}

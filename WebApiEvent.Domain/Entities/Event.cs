using WebApiEvent.Domain.Exceptions;

namespace WebApiEvent.Domain.Entities
{
    public class Event : BaseEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public DateTime StartAt { get; private set; }
        public DateTime EndAt { get; private set; }
        public bool IsActive { get; private set; } = true;

        public ICollection<Booking> Bookings { get; private set; } = new List<Booking>();

        private Event() { }

        public static Event Create(string title, string description, DateTime startAt, DateTime endAt)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Заголовок обязателен");

            if (startAt >= endAt)
                throw new DomainException("Дата окончания должна быть позже даты начала");

            return new Event
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                StartAt = startAt,
                EndAt = endAt
            };
        }

        public void Update(string title, string description, DateTime startAt, DateTime endAt)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Заголовок обязателен");

            if (startAt >= endAt)
                throw new DomainException("Дата окончания должна быть позже даты начала");

            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }

        public void Deactivate() => IsActive = false;
    }
}

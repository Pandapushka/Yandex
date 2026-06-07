using WebApiEvent.CustomExceptions;

namespace WebApiEvent.Models.Entity
{
    public class Event : BaseEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public DateTime StartAt { get; private set; }
        public DateTime EndAt { get; private set; }
        public bool IsActive { get; private set; } = true;
        public int TotalSeats { get; private set; }
        public int AvailableSeats { get; private set; }

        private Event() { }

        public static Event Create(string title, string description, DateTime startAt, DateTime endAt, int totalSeats)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Заголовок обязателен");

            if (startAt >= endAt)
                throw new DomainException("Дата окончания должна быть позже даты начала");

            if (totalSeats <= 0)
                throw new DomainException("Количество мест должно быть больше нуля");

            return new Event
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                StartAt = startAt,
                EndAt = endAt,
                TotalSeats = totalSeats,
                AvailableSeats = totalSeats
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

        public bool TryReserveSeats(int count = 1)
        {
            if (count <= 0) return false;
            if (AvailableSeats < count) return false;
            AvailableSeats -= count;
            return true;
        }

        public void ReleaseSeats(int count = 1)
        {
            if (count <= 0) return;
            int newAvailable = AvailableSeats + count;
            if (newAvailable > TotalSeats) newAvailable = TotalSeats;
            AvailableSeats = newAvailable;
        }
    }
}
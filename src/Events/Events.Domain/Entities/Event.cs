using Events.Domain.Exceptions;

namespace Events.Domain.Entities
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

        public static Event Create(string title, string description, DateTime startAt, DateTime endAt, int availableSeats)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Заголовок обязателен");

            if (startAt >= endAt)
                throw new DomainException("Дата окончания должна быть позже даты начала");

            if (availableSeats < 0)
                throw new DomainException("Количество доступных мест не может быть отрицательным");

            return new Event
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                StartAt = startAt,
                EndAt = endAt,
                TotalSeats = availableSeats,
                AvailableSeats = availableSeats
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

        public void DecreaseSeats(int count)
        {
            if (count < 0)
                throw new DomainException("Количество мест не может быть отрицательным");

            if (count > AvailableSeats)
                throw new NoAvailableSeatsException($"Недостаточно свободных мест. Доступно: {AvailableSeats}");

            AvailableSeats -= count;
        }
    }
}

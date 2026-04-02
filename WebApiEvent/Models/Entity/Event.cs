namespace WebApiEvent.Models.Entity
{
    public class Event : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public bool IsActiv { get; set; } = true;
        private Event() {}

        public static Event Create(string title, string description, DateTime startAt, DateTime endAt)
        {
            if (title == null || title == string.Empty)
                throw new Exception("Описание должно быть заполнено");
            if (startAt < DateTime.Now)
                throw new Exception("Не корректная дата старта мероприятия");
            if (startAt <= endAt)
                throw new Exception("Не корректная дата завершения мероприятия");
            Event Event = new()
            {
                Title = title,
                Description = description,
                StartAt = startAt,
                EndAt = endAt
            };
            return Event;
        }
    }
}

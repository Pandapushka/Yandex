namespace WebApiEvent.Models.Entity
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } //set временный, до тех пор пока не подключим бд
    }
}

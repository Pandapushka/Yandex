namespace Events.Domain.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base($"NotFoundException: {message}") { }
    }
}

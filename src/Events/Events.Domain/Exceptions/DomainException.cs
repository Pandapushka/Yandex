namespace Events.Domain.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base($"DomainException: {message}") { }
    }
}

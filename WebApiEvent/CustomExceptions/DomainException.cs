namespace WebApiEvent.CustomExceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base($"DomainException: {message}") { }
    }
}
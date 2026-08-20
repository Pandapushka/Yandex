namespace WebApiEvent.Domain.Exceptions
{
    public class ServiceException : Exception
    {
        public ServiceException(string message) : base($"ServiceExceptions: {message}") { }
    }
}

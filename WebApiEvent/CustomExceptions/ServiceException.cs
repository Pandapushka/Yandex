namespace WebApiEvent.CustomExceptions
{
    public class ServiceException : Exception
    {
        public ServiceException(string message) : base($"ServiceExceptions: {message}") { }
    }
}

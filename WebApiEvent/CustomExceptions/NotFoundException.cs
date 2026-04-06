namespace WebApiEvent.CustomExceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base($"NotFoundException: {message}") { }
    }
}

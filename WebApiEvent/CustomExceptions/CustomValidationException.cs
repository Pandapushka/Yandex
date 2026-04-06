namespace WebApiEvent.CustomExceptions
{
    public class CustomValidationException : Exception
    {
        public CustomValidationException(string message) : base($"ValidationException: {message}") { }
    }
}

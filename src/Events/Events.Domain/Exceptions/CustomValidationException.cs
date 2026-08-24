namespace Events.Domain.Exceptions
{
    public class CustomValidationException : Exception
    {
        public CustomValidationException(string message) : base($"ValidationException: {message}") { }
    }
}

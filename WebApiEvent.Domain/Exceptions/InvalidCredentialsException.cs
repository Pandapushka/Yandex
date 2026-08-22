namespace WebApiEvent.Domain.Exceptions
{
    /// <summary>Неверный логин или пароль (аутентификация не удалась) → 401.</summary>
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string message) : base(message) { }
    }
}

namespace WebApiEvent.Application.DTOs.Auth
{
    /// <summary>Учётные данные для входа.</summary>
    public record LoginRequest(string Login, string Password);
}

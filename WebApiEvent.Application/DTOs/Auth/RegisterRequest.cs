namespace WebApiEvent.Application.DTOs.Auth
{
    /// <summary>Запрос на регистрацию. Роль определяется вызываемым эндпоинтом.</summary>
    public record RegisterRequest(string Login, string Password);
}

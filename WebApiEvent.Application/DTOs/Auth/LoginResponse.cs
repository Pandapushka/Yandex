namespace WebApiEvent.Application.DTOs.Auth
{
    /// <summary>Ответ при успешном входе — JWT-токен.</summary>
    public record LoginResponse(string Token);
}

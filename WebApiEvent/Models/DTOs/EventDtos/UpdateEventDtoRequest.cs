using System.ComponentModel.DataAnnotations;

namespace WebApiEvent.Models.DTOs.EventDtos
{
    public record UpdateEventDtoRequest
    (
        [MinLength(1, ErrorMessage = "Заголовок не может быть пустым")]
        [MaxLength(200, ErrorMessage = "Заголовок не может превышать 200 символов")]
        string? Title = null,

        [MaxLength(1000, ErrorMessage = "Описание не может превышать 1000 символов")]
        string? Description = null,

        DateTime? StartAt = null,

        DateTime? EndAt = null
    );
}
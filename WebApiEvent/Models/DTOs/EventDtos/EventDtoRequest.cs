using System.ComponentModel.DataAnnotations;

namespace WebApiEvent.Models.DTOs.EventDtos
{
    public record EventDtoRequest
    (
        [Required(ErrorMessage = "Заголовок обязателен")]
        [MinLength(1, ErrorMessage = "Заголовок не может быть пустым")]
        [MaxLength(200, ErrorMessage = "Заголовок не может превышать 200 символов")]
        string Title,

        [MaxLength(1000, ErrorMessage = "Описание не может превышать 1000 символов")]
        string? Description,

        [Required(ErrorMessage = "Дата начала обязательна")]
        DateTime StartAt,

        [Required(ErrorMessage = "Дата окончания обязательна")]
        DateTime EndAt
    );
}
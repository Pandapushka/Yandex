using Microsoft.AspNetCore.Mvc;
using WebApiEvent.CustomExceptions;
using WebApiEvent.Models.DTOs;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Services;

namespace WebApiEvent.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController(IEventService _eventService) : ControllerBase
    {
        [HttpGet]
        public ActionResult<ResponseServerDto<PaginatedResult<EventDtoResponse>>> GetAll([FromQuery] EventRequestDto request)
        {
            var result = _eventService.GetAll(request);
            return Ok(ResponseServerDto<PaginatedResult<EventDtoResponse>>.Success(result, 200));
        }

        [HttpGet("{id:guid}")]
        public ActionResult<ResponseServerDto<EventDtoResponse>> GetById(Guid id)
        {
            var result = _eventService.GetById(id);
            return Ok(ResponseServerDto<EventDtoResponse>.Success(result, 200));
        }

        [HttpPost]
        public ActionResult<ResponseServerDto<string>> Create([FromBody] EventDtoRequest request)
        {
            var id = _eventService.Create(request);
            return CreatedAtAction(nameof(GetById), new { id }, ResponseServerDto<string>.Success($"Событие c id {id} успешно создано", 201));
        }

        [HttpPut("{id:guid}")]
        public ActionResult<ResponseServerDto<string>> Update(Guid id, [FromBody] UpdateEventDtoRequest request)
        {
            _eventService.Update(id, request);
            return Ok(ResponseServerDto<string>.Success("Событие успешно обновлено", 200));
        }

        [HttpDelete("{id:guid}")]
        public ActionResult<ResponseServerDto<string>> Delete(Guid id)
        {
            _eventService.Delete(id);
            return Ok(ResponseServerDto<string>.Success("Событие успешно удалено", 200));
        }

        [HttpPatch("{id:guid}/soft-delete")]
        public ActionResult<ResponseServerDto<string>> SoftDelete(Guid id)
        {
            _eventService.SoftDelete(id);
            return Ok(ResponseServerDto<string>.Success("Событие успешно деактивировано", 200));
        }
    }
}
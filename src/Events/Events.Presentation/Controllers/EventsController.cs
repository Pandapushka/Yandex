using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Events.Application.DTOs;
using Events.Application.DTOs.Event;
using Events.Application.Interfaces;

namespace Events.Presentation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController(IEventService _eventService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<ResponseServerDto<PaginatedResult<EventDtoResponse>>>> GetAll([FromQuery] EventRequestDto request)
        {
            var result = await _eventService.GetAllAsync(request);
            return Ok(ResponseServerDto<PaginatedResult<EventDtoResponse>>.Success(result, 200));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ResponseServerDto<EventDtoResponse>>> GetById(Guid id)
        {
            var result = await _eventService.GetByIdAsync(id);
            return Ok(ResponseServerDto<EventDtoResponse>.Success(result, 200));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ResponseServerDto<string>>> Create([FromBody] EventDtoRequest request)
        {
            var id = await _eventService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id },
                ResponseServerDto<string>.Success($"Событие c id {id} успешно создано", 201));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ResponseServerDto<string>>> Update(Guid id, [FromBody] UpdateEventDtoRequest request)
        {
            await _eventService.UpdateAsync(id, request);
            return Ok(ResponseServerDto<string>.Success("Событие успешно обновлено", 200));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ResponseServerDto<string>>> Delete(Guid id)
        {
            await _eventService.DeleteAsync(id);
            return Ok(ResponseServerDto<string>.Success("Событие успешно удалено", 200));
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}/soft-delete")]
        public async Task<ActionResult<ResponseServerDto<string>>> SoftDelete(Guid id)
        {
            await _eventService.SoftDeleteAsync(id);
            return Ok(ResponseServerDto<string>.Success("Событие успешно деактивировано", 200));
        }
    }
}

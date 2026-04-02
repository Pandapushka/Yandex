using Microsoft.AspNetCore.Mvc;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Services;

namespace WebApiEvent.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController(IEventService _eventService) : ControllerBase
    {
        
        [HttpGet]
        public ActionResult<List<EventDtoResponse>> GetAll()
        {
            return Ok(_eventService.GetAll());
        }

        [HttpGet("{id:guid}")]
        public ActionResult<EventDtoResponse> GetById(Guid id)
        {
            var result = _eventService.GetById(id);
            return result is not null ? Ok(result) : NotFound();
        }

        [HttpPost]
        public ActionResult<EventDtoResponse> Create([FromBody] EventDtoRequest request)
        {
            var id = _eventService.Create(request);
            var response = _eventService.GetById(id);
            return CreatedAtAction(nameof(GetById), new { id }, response);
        }

        [HttpPut("{id:guid}")]
        public IActionResult Update(Guid id, [FromBody] UpdateEventDtoRequest request)
        {
            _eventService.Update(id, request);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            _eventService.Delete(id);
            return NoContent();
        }

        [HttpPatch("{id:guid}/soft-delete")]
        public IActionResult SoftDelete(Guid id)
        {
            _eventService.SoftDelete(id);
            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using WebApiEvent.Models.DTOs;
using WebApiEvent.Models.DTOs.EventDtos;
using WebApiEvent.Services;

namespace WebApiEvent.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController(IEventService _eventService) : ControllerBase
    {
        [HttpGet("GetAll")]
        public ActionResult<ResponseServerDto<List<EventDtoResponse>>> GetAll()
        {
            try
            {
                var result = _eventService.GetAll();
                return Ok(ResponseServerDto<List<EventDtoResponse>>.Success(result, 200));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseServerDto<List<EventDtoResponse>>.Error(ex.Message, 500));
            }
        }

        [HttpGet("GetById")]
        public ActionResult<ResponseServerDto<EventDtoResponse>> GetById(Guid id)
        {
            try
            {
                var result = _eventService.GetById(id);
                return result is not null
                    ? Ok(ResponseServerDto<EventDtoResponse>.Success(result, 200))
                    : NotFound(ResponseServerDto<EventDtoResponse>.Error("Событие не найдено", 404));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseServerDto<EventDtoResponse>.Error(ex.Message, 500));
            }
        }

        [HttpPost("Create")]
        public ActionResult<ResponseServerDto<string>> Create([FromBody] EventDtoRequest request)
        {
            try
            {
                var id = _eventService.Create(request);
                return Ok(ResponseServerDto<string>.Success($"Событие c id {id}  успешно создано", 201));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseServerDto<string>.Error(ex.Message, 500));
            }
        }

        [HttpPut("Update")]
        public ActionResult<ResponseServerDto<string>> Update(Guid id, [FromBody] UpdateEventDtoRequest request)
        {
            try
            {
                _eventService.Update(id, request);
                return Ok(ResponseServerDto<string>.Success("Событие успешно обновлено", 200));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseServerDto<string>.Error(ex.Message, 500));
            }
        }

        [HttpDelete("Delete")]
        public ActionResult<ResponseServerDto<string>> Delete(Guid id)
        {
            try
            {
                _eventService.Delete(id);
                return Ok(ResponseServerDto<string>.Success("Событие успешно удалено", 200));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseServerDto<string>.Error(ex.Message, 500));
            }
        }

        [HttpPatch("SoftDelete")]
        public ActionResult<ResponseServerDto<string>> SoftDelete(Guid id)
        {
            try
            {
                _eventService.SoftDelete(id);
                return Ok(ResponseServerDto<string>.Success("Событие успешно деактивировано", 200));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseServerDto<string>.Error(ex.Message, 500));
            }
        }
    }
}
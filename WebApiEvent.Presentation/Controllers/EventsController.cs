using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiEvent.Application.DTOs;
using WebApiEvent.Application.DTOs.Booking;
using WebApiEvent.Application.DTOs.Event;
using WebApiEvent.Application.Interfaces;

namespace WebApiEvent.Presentation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventsController(IEventService _eventService, IBookingService _bookingService) : ControllerBase
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

        [Authorize]
        [HttpPost("{id:guid}/book")]
        public async Task<ActionResult<ResponseServerDto<BookingResponse>>> BookEvent(Guid id, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var booking = await _bookingService.CreateBookingAsync(userId, id, cancellationToken);
            return AcceptedAtAction(
                actionName: nameof(BookingsController.GetBooking),
                controllerName: "Bookings",
                routeValues: new { id = booking.Id },
                value: ResponseServerDto<BookingResponse>.Success(booking, 202)
            );
        }
    }
}

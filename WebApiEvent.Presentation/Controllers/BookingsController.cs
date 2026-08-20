using Microsoft.AspNetCore.Mvc;
using WebApiEvent.Application.DTOs;
using WebApiEvent.Application.DTOs.Booking;
using WebApiEvent.Application.Interfaces;

namespace WebApiEvent.Presentation.Controllers
{
    [ApiController]
    [Route("bookings")]
    public class BookingsController(IBookingService _bookingService) : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ResponseServerDto<BookingResponse>>> GetBooking(Guid id)
        {
            var result = await _bookingService.GetBookingAsync(id);
            return Ok(ResponseServerDto<BookingResponse>.Success(result, 200));
        }
    }
}

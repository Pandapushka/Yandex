using Microsoft.AspNetCore.Mvc;
using WebApiEvent.Models.DTOs;
using WebApiEvent.Models.DTOs.BookingDtos;
using WebApiEvent.Services;

namespace WebApiEvent.Controllers
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
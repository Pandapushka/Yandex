using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ResponseServerDto<BookingResponse>>> GetBooking(Guid id, CancellationToken cancellationToken)
        {
            var result = await _bookingService.GetBookingAsync(id, cancellationToken);
            return Ok(ResponseServerDto<BookingResponse>.Success(result, 200));
        }

        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteBooking(Guid id, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdmin = User.IsInRole("Admin");

            await _bookingService.CancelBookingAsync(id, userId, isAdmin, cancellationToken);
            return NoContent();
        }
    }
}

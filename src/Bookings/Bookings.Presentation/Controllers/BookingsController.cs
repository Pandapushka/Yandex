using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookings.Application.DTOs;
using Bookings.Application.DTOs.Booking;
using Bookings.Application.Interfaces;

namespace Bookings.Presentation.Controllers
{
    [ApiController]
    [Route("bookings")]
    public class BookingsController(IBookingService _bookingService) : ControllerBase
    {
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ResponseServerDto<BookingResponse>>> CreateBooking(
            [FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var booking = await _bookingService.CreateBookingAsync(userId, request.EventId, cancellationToken);

            return AcceptedAtAction(
                nameof(GetBooking),
                new { id = booking.Id },
                ResponseServerDto<BookingResponse>.Success(booking, 202));
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ResponseServerDto<BookingResponse>>> GetBooking(Guid id, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdmin = User.IsInRole("Admin");

            var result = await _bookingService.GetBookingAsync(id, userId, isAdmin, cancellationToken);
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

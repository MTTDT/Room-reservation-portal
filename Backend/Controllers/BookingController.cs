using Backend.DTOs.Bookings;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController(IBookingService bookingService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllBookings(CancellationToken ct)
        {
            var bookings = await bookingService.GetAllBookingsAsync(ct);
            return Ok(bookings);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBookingById(int id, CancellationToken ct)
        {
            var booking = await bookingService.GetBookingByIdAsync(id, ct);
            if (booking == null)
            {
                return NotFound();
            }
            return Ok(booking);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdBooking = await bookingService.CreateBookingAsync(request, ct);
            return CreatedAtAction(nameof(GetBookingById), new { id = createdBooking.Id }, createdBooking);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] UpdateBookingRequest request, CancellationToken ct)
        {
            var updatedBooking = await bookingService.UpdateBookingAsync(id, request, ct);
            if (updatedBooking == null)
            {
                return NotFound();
            }
            return Ok(updatedBooking);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBooking(int id, CancellationToken ct)
        {
            await bookingService.DeleteBookingAsync(id, ct);
            return NoContent();
        }
    }
}
using Backend.DTOs.Reservations;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController(IReservationService reservationService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllReservations(CancellationToken ct)
        {
            var reservations = await reservationService.GetAllReservationsAsync(ct);
            return Ok(reservations);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetReservationById(int id, CancellationToken ct)
        {
            var reservation = await reservationService.GetReservationByIdAsync(id, ct);
            if (reservation == null)
            {
                return NotFound();
            }
            return Ok(reservation);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request, CancellationToken ct)
        {
            Console.WriteLine("got in");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdReservation = await reservationService.CreateReservationAsync(request, ct);
            Console.WriteLine("got in and created " + createdReservation.Id);

            return CreatedAtAction(nameof(GetReservationById), new { id = createdReservation.Id }, createdReservation);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationRequest request, CancellationToken ct)
        {
            var updatedReservation = await reservationService.UpdateReservationAsync(id, request, ct);
            if (updatedReservation == null)
            {
                return NotFound();
            }
            return Ok(updatedReservation);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteReservation(int id, CancellationToken ct)
        {
            await reservationService.DeleteReservationAsync(id, ct);
            return NoContent();
        }
    }
}
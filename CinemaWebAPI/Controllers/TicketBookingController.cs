using CinemaWebAPI.Dtos;
using CinemaWebAPI.Models;
using CinemaWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaWebAPI.Controllers
{
    [ApiController]
    [Route("api/ticketBookings")]
    [Authorize]
    public class TicketBookingController : ControllerBase
    {
        private readonly ITicketBookingService _service;

        public TicketBookingController(ITicketBookingService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<IEnumerable<TicketBookingResponseDto>>> GetAll([FromQuery] string? movieSessionId)
        {
            return Ok(await _service.GetAllAsync(movieSessionId));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketBookingResponseDto>> GetById(string id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        [HttpGet("barMenu")]
        [AllowAnonymous]
        public ActionResult<IEnumerable<BarMenuItemDto>> GetBarMenu([FromQuery] string? sort)
        {
            return Ok(_service.GetBarMenu(sort));
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<TicketBookingResponseDto>> Update(string id, UpdateTicketBookingDto dto)
        {
            return Ok(await _service.UpdateAsync(id, dto));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Delete(string id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("reserve")]
        public async Task<ActionResult<TicketBookingResponseDto>> Reserve(ReserveBookingRequestDto dto)
        {
            TicketBookingResponseDto booking = await _service.ReserveAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }

        [HttpPost("{id}/pay")]
        public async Task<ActionResult<TicketBookingResponseDto>> Pay(string id)
        {
            return Ok(await _service.PayAsync(id));
        }

        [HttpPost("{id}/refund")]
        public async Task<ActionResult<TicketBookingResponseDto>> Refund(string id)
        {
            return Ok(await _service.RefundAsync(id));
        }

        [HttpGet("sessions/{movieSessionId}/statistics")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<BookingStatisticsDto>> GetStatistics(string movieSessionId)
        {
            return Ok(await _service.GetStatisticsAsync(movieSessionId));
        }

        [HttpGet("revenue")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<MonthlyRevenueDto>> GetMonthlyRevenue([FromQuery] int year, [FromQuery] int month)
        {
            decimal total = await _service.GetMonthlyRevenueAsync(year, month);
            return Ok(new MonthlyRevenueDto { Year = year, Month = month, TotalRevenue = total });
        }

        [HttpPost("sessions/{movieSessionId}/cancel")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<CancelSessionResultDto>> CancelSession(string movieSessionId)
        {
            return Ok(await _service.CancelSessionAsync(movieSessionId));
        }
    }
}

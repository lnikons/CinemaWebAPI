using CinemaWebAPI.Dtos;
using CinemaWebAPI.Models;
using CinemaWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaWebAPI.Controllers
{
    [ApiController]
    [Route("api/cinemaHallSeats")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class CinemaHallSeatController : ControllerBase
    {
        private readonly ICinemaHallSeatService _service;

        public CinemaHallSeatController(ICinemaHallSeatService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CinemaHallSeatResponseDto>>> GetAll([FromQuery] string? movieSessionId)
        {
            return Ok(await _service.GetAllAsync(movieSessionId));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CinemaHallSeatResponseDto>> GetById(string id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult<CinemaHallSeatResponseDto>> Create(CreateCinemaHallSeatDto dto)
        {
            CinemaHallSeatResponseDto created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CinemaHallSeatResponseDto>> Update(string id, UpdateCinemaHallSeatDto dto)
        {
            return Ok(await _service.UpdateAsync(id, dto));
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<CinemaHallSeatResponseDto>> Patch(string id, PatchCinemaHallSeatDto dto)
        {
            return Ok(await _service.PatchAsync(id, dto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}

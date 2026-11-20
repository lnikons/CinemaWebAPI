using CinemaWebAPI.Dtos;
using CinemaWebAPI.Models;
using CinemaWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaWebAPI.Controllers
{
    [ApiController]
    [Route("api/movieSessions")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class MovieSessionController : ControllerBase
    {
        private readonly IMovieSessionService _service;

        public MovieSessionController(IMovieSessionService service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MovieSessionResponseDto>>> GetAll([FromQuery] string? sort)
        {
            return Ok(await _service.GetAllAsync(sort));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<MovieSessionResponseDto>> GetById(string id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult<MovieSessionResponseDto>> Create(CreateMovieSessionDto dto)
        {
            MovieSessionResponseDto created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<MovieSessionResponseDto>> Update(string id, UpdateMovieSessionDto dto)
        {
            return Ok(await _service.UpdateAsync(id, dto));
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<MovieSessionResponseDto>> Patch(string id, PatchMovieSessionDto dto)
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

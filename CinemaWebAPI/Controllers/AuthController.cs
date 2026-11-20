using System.Security.Claims;
using CinemaWebAPI.Dtos;
using CinemaWebAPI.Models;
using CinemaWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaWebAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(RegisterRequestDto dto)
        {
            UserResponseDto user = await _service.RegisterAsync(dto);
            return CreatedAtAction(nameof(Me), null, user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto dto)
        {
            return Ok(await _service.LoginAsync(dto));
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenRequestDto dto)
        {
            return Ok(await _service.RefreshAsync(dto));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserResponseDto>> Me()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            return Ok(await _service.GetCurrentUserAsync(userId));
        }

        [HttpGet("users")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            return Ok(await _service.GetAllUsersAsync());
        }

        [HttpPatch("users/{id}/role")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<ActionResult<UserResponseDto>> ChangeRole(string id, ChangeUserRoleDto dto)
        {
            return Ok(await _service.ChangeRoleAsync(id, dto.Role));
        }
    }
}

using CinemaWebAPI.Models;

namespace CinemaWebAPI.Dtos
{
    public class RegisterRequestDto
    {
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginRequestDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class RefreshTokenRequestDto
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
    }

    public class ChangeUserRoleDto
    {
        public UserRole Role { get; set; }
    }

    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = "";
        public DateTime AccessTokenExpiresAt { get; set; }
        public string RefreshToken { get; set; } = "";
        public DateTime RefreshTokenExpiresAt { get; set; }
        public UserResponseDto User { get; set; } = new UserResponseDto();
    }

    public class UserResponseDto
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

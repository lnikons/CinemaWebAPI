using System.Security.Claims;
using AutoMapper;
using CinemaWebAPI.Dtos;
using CinemaWebAPI.Exceptions;
using CinemaWebAPI.Models;
using CinemaWebAPI.Repositories;
using CinemaWebAPI.Settings;
using Microsoft.Extensions.Options;

namespace CinemaWebAPI.Services
{
    public interface IAuthService
    {
        Task<UserResponseDto> RegisterAsync(RegisterRequestDto dto);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
        Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto dto);
        Task<UserResponseDto> GetCurrentUserAsync(string userId);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto> ChangeRoleAsync(string userId, UserRole role);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly ITokenService _tokens;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;

        public AuthService(IUserRepository users, ITokenService tokens, IOptions<JwtSettings> jwtOptions, IMapper mapper)
        {
            _users = users;
            _tokens = tokens;
            _jwtSettings = jwtOptions.Value;
            _mapper = mapper;
        }

        public async Task<UserResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            if (await _users.GetByEmailAsync(dto.Email) != null)
                throw new BusinessRuleException($"Користувач з email {dto.Email} уже зареєстрований.");

            User user = _mapper.Map<User>(dto);
            user.Email = dto.Email.Trim().ToLowerInvariant();
            user.PasswordHash = PasswordHasher.Hash(dto.Password);
            user.Role = UserRole.User;
            user.CreatedAt = DateTime.UtcNow;

            User created = await _users.CreateAsync(user);
            return _mapper.Map<UserResponseDto>(created);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            User? user = await _users.GetByEmailAsync(dto.Email);
            if (user == null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedException("Неправильний email або пароль.");

            return await IssueTokensAsync(user);
        }

        public async Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto dto)
        {
            ClaimsPrincipal? principal = _tokens.GetPrincipalFromExpiredToken(dto.AccessToken);
            string? userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                throw new UnauthorizedException("Некоректний access token.");

            User? user = await _users.GetByIdAsync(userId);
            if (user == null || user.RefreshToken == null || user.RefreshToken != dto.RefreshToken)
                throw new UnauthorizedException("Refresh token не належить користувачу або вже був використаний.");

            if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedException("Строк дії refresh token минув. Увійдіть повторно.");

            return await IssueTokensAsync(user);
        }

        public async Task<UserResponseDto> GetCurrentUserAsync(string userId)
        {
            User user = await _users.GetByIdAsync(userId)
                ?? throw new NotFoundException($"Користувача з ID={userId} не знайдено.");
            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            return _mapper.Map<IEnumerable<UserResponseDto>>(await _users.GetAllAsync());
        }

        public async Task<UserResponseDto> ChangeRoleAsync(string userId, UserRole role)
        {
            User user = await _users.GetByIdAsync(userId)
                ?? throw new NotFoundException($"Користувача з ID={userId} не знайдено.");

            user.Role = role;
            await _users.UpdateAsync(user);
            return _mapper.Map<UserResponseDto>(user);
        }

        private async Task<AuthResponseDto> IssueTokensAsync(User user)
        {
            (string accessToken, DateTime accessExpiresAt) = _tokens.CreateAccessToken(user);

            user.RefreshToken = _tokens.CreateRefreshToken();
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);
            await _users.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExpiresAt,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpiresAt = user.RefreshTokenExpiresAt.Value,
                User = _mapper.Map<UserResponseDto>(user)
            };
        }
    }
}

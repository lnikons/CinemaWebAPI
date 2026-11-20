using AutoMapper;
using CinemaWebAPI.Dtos;
using CinemaWebAPI.Exceptions;
using CinemaWebAPI.Models;
using CinemaWebAPI.Repositories;

namespace CinemaWebAPI.Services
{
    public interface IMovieSessionService
    {
        Task<IEnumerable<MovieSessionResponseDto>> GetAllAsync(string? sort);
        Task<MovieSessionResponseDto> GetByIdAsync(string id);
        Task<MovieSessionResponseDto> CreateAsync(CreateMovieSessionDto dto);
        Task<MovieSessionResponseDto> UpdateAsync(string id, UpdateMovieSessionDto dto);
        Task<MovieSessionResponseDto> PatchAsync(string id, PatchMovieSessionDto dto);
        Task DeleteAsync(string id);
    }

    public class MovieSessionService : IMovieSessionService
    {
        private readonly IMovieSessionRepository _sessions;
        private readonly IMapper _mapper;

        public MovieSessionService(IMovieSessionRepository sessions, IMapper mapper)
        {
            _sessions = sessions;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MovieSessionResponseDto>> GetAllAsync(string? sort)
        {
            IEnumerable<MovieSession> sessions = await _sessions.GetAllAsync();

            switch (sort?.Trim().ToLowerInvariant())
            {
                case null:
                case "":
                    break;
                case "starttime":
                    sessions = sessions.OrderBy(s => s.StartTime);
                    break;
                case "-starttime":
                    sessions = sessions.OrderByDescending(s => s.StartTime);
                    break;
                default:
                    throw new BadRequestException($"Невідоме значення sort='{sort}'. Допустимі значення: startTime, -startTime.");
            }

            return _mapper.Map<IEnumerable<MovieSessionResponseDto>>(sessions);
        }

        public async Task<MovieSessionResponseDto> GetByIdAsync(string id)
        {
            return _mapper.Map<MovieSessionResponseDto>(await GetExistingAsync(id));
        }

        public async Task<MovieSessionResponseDto> CreateAsync(CreateMovieSessionDto dto)
        {
            MovieSession session = _mapper.Map<MovieSession>(dto);
            session.Status = MovieSessionStatus.Scheduled;

            MovieSession created = await _sessions.CreateAsync(session);
            return _mapper.Map<MovieSessionResponseDto>(created);
        }

        public async Task<MovieSessionResponseDto> UpdateAsync(string id, UpdateMovieSessionDto dto)
        {
            MovieSession session = await GetExistingAsync(id);
            _mapper.Map(dto, session);

            await _sessions.UpdateAsync(session);
            return _mapper.Map<MovieSessionResponseDto>(session);
        }

        public async Task<MovieSessionResponseDto> PatchAsync(string id, PatchMovieSessionDto dto)
        {
            MovieSession session = await GetExistingAsync(id);
            _mapper.Map(dto, session);

            await _sessions.UpdateAsync(session);
            return _mapper.Map<MovieSessionResponseDto>(session);
        }

        public async Task DeleteAsync(string id)
        {
            MovieSession session = await GetExistingAsync(id);

            if (session.Status == MovieSessionStatus.Scheduled || session.Status == MovieSessionStatus.Ongoing)
                throw new BusinessRuleException($"Видалити можна лише скасований або завершений сеанс. Поточний статус: {session.Status}.");

            await _sessions.DeleteAsync(id);
        }

        private async Task<MovieSession> GetExistingAsync(string id)
        {
            return await _sessions.GetByIdAsync(id)
                ?? throw new NotFoundException($"Сеанс з ID={id} не знайдено.");
        }
    }
}

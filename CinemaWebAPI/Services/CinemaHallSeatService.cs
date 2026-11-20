using AutoMapper;
using CinemaWebAPI.Dtos;
using CinemaWebAPI.Exceptions;
using CinemaWebAPI.Models;
using CinemaWebAPI.Repositories;

namespace CinemaWebAPI.Services
{
    public interface ICinemaHallSeatService
    {
        Task<IEnumerable<CinemaHallSeatResponseDto>> GetAllAsync(string? movieSessionId);
        Task<CinemaHallSeatResponseDto> GetByIdAsync(string id);
        Task<CinemaHallSeatResponseDto> CreateAsync(CreateCinemaHallSeatDto dto);
        Task<CinemaHallSeatResponseDto> UpdateAsync(string id, UpdateCinemaHallSeatDto dto);
        Task<CinemaHallSeatResponseDto> PatchAsync(string id, PatchCinemaHallSeatDto dto);
        Task DeleteAsync(string id);
    }

    public class CinemaHallSeatService : ICinemaHallSeatService
    {
        private readonly ICinemaHallSeatRepository _seats;
        private readonly IMapper _mapper;

        public CinemaHallSeatService(ICinemaHallSeatRepository seats, IMapper mapper)
        {
            _seats = seats;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CinemaHallSeatResponseDto>> GetAllAsync(string? movieSessionId)
        {
            IReadOnlyList<CinemaHallSeat> seats = string.IsNullOrWhiteSpace(movieSessionId)
                ? await _seats.GetAllAsync()
                : await _seats.GetBySessionIdAsync(movieSessionId);

            return _mapper.Map<IEnumerable<CinemaHallSeatResponseDto>>(seats);
        }

        public async Task<CinemaHallSeatResponseDto> GetByIdAsync(string id)
        {
            return _mapper.Map<CinemaHallSeatResponseDto>(await GetExistingAsync(id));
        }

        public async Task<CinemaHallSeatResponseDto> CreateAsync(CreateCinemaHallSeatDto dto)
        {
            await EnsureSeatIsUniqueAsync(dto.MovieSessionId, dto.RowNumber, dto.SeatNumber, null);

            CinemaHallSeat seat = _mapper.Map<CinemaHallSeat>(dto);
            seat.Status = SeatStatus.Free;

            CinemaHallSeat created = await _seats.CreateAsync(seat);
            return _mapper.Map<CinemaHallSeatResponseDto>(created);
        }

        public async Task<CinemaHallSeatResponseDto> UpdateAsync(string id, UpdateCinemaHallSeatDto dto)
        {
            CinemaHallSeat seat = await GetExistingAsync(id);
            await EnsureSeatIsUniqueAsync(seat.MovieSessionId, dto.RowNumber, dto.SeatNumber, id);

            _mapper.Map(dto, seat);
            await _seats.UpdateAsync(seat);
            return _mapper.Map<CinemaHallSeatResponseDto>(seat);
        }

        public async Task<CinemaHallSeatResponseDto> PatchAsync(string id, PatchCinemaHallSeatDto dto)
        {
            CinemaHallSeat seat = await GetExistingAsync(id);
            await EnsureSeatIsUniqueAsync(seat.MovieSessionId, dto.RowNumber ?? seat.RowNumber, dto.SeatNumber ?? seat.SeatNumber, id);

            _mapper.Map(dto, seat);
            await _seats.UpdateAsync(seat);
            return _mapper.Map<CinemaHallSeatResponseDto>(seat);
        }

        public async Task DeleteAsync(string id)
        {
            CinemaHallSeat seat = await GetExistingAsync(id);

            if (seat.Status != SeatStatus.Free)
                throw new BusinessRuleException($"Місце з ID={id} має статус {seat.Status} і входить в активне замовлення, його неможливо видалити.");

            await _seats.DeleteAsync(id);
        }

        private async Task<CinemaHallSeat> GetExistingAsync(string id)
        {
            return await _seats.GetByIdAsync(id)
                ?? throw new NotFoundException($"Місце з ID={id} не знайдено.");
        }

        private async Task EnsureSeatIsUniqueAsync(string movieSessionId, int rowNumber, int seatNumber, string? exceptId)
        {
            if (await _seats.ExistsAsync(movieSessionId, rowNumber, seatNumber, exceptId))
                throw new BusinessRuleException($"Місце ряд {rowNumber}, місце {seatNumber} уже існує на цьому сеансі.");
        }
    }
}

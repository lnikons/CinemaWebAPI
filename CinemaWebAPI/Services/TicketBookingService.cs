using AutoMapper;
using CinemaWebAPI.Dtos;
using CinemaWebAPI.Exceptions;
using CinemaWebAPI.Models;
using CinemaWebAPI.Repositories;

namespace CinemaWebAPI.Services
{
    public interface ITicketBookingService
    {
        Task<IEnumerable<TicketBookingResponseDto>> GetAllAsync(string? movieSessionId);
        Task<TicketBookingResponseDto> GetByIdAsync(string id);
        IEnumerable<BarMenuItemDto> GetBarMenu(string? sort);
        Task<TicketBookingResponseDto> UpdateAsync(string id, UpdateTicketBookingDto dto);
        Task DeleteAsync(string id);

        Task<TicketBookingResponseDto> ReserveAsync(ReserveBookingRequestDto dto);
        Task<TicketBookingResponseDto> PayAsync(string id);
        Task<TicketBookingResponseDto> RefundAsync(string id);
        Task<CancelSessionResultDto> CancelSessionAsync(string movieSessionId);
        Task<BookingStatisticsDto> GetStatisticsAsync(string movieSessionId);
        Task<decimal> GetMonthlyRevenueAsync(int year, int month);
    }

    public class TicketBookingService : ITicketBookingService
    {
        private const int HoldMinutes = 10;
        private const int BookingClosesMinutes = 15;
        private const int RefundClosesMinutes = 30;
        private const int MorningShowEndHour = 12;
        private const decimal MorningStandardMultiplier = 0.75m;
        private const decimal VipMultiplier = 1.5m;
        private const int FamilyTicketsCount = 4;
        private const decimal FamilyBarDiscount = 0.15m;
        private const decimal CompensationBonus = 100;
        private const int MinReportYear = 2000;
        private const int MaxReportYear = 2100;

        private readonly ITicketBookingRepository _bookings;
        private readonly IMovieSessionRepository _sessions;
        private readonly ICinemaHallSeatRepository _seats;
        private readonly IMapper _mapper;

        public TicketBookingService(ITicketBookingRepository bookings, IMovieSessionRepository sessions,
            ICinemaHallSeatRepository seats, IMapper mapper)
        {
            _bookings = bookings;
            _sessions = sessions;
            _seats = seats;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TicketBookingResponseDto>> GetAllAsync(string? movieSessionId)
        {
            await ReleaseExpiredBookingsAsync();

            IReadOnlyList<TicketBooking> bookings = string.IsNullOrWhiteSpace(movieSessionId)
                ? await _bookings.GetAllAsync()
                : await _bookings.GetBySessionIdAsync(movieSessionId);

            return _mapper.Map<IEnumerable<TicketBookingResponseDto>>(bookings);
        }

        public async Task<TicketBookingResponseDto> GetByIdAsync(string id)
        {
            await ReleaseExpiredBookingsAsync();
            return _mapper.Map<TicketBookingResponseDto>(await GetExistingAsync(id));
        }

        public IEnumerable<BarMenuItemDto> GetBarMenu(string? sort)
        {
            IEnumerable<BarMenuItemDto> items = BarMenu.Prices
                .Select(p => new BarMenuItemDto { Product = p.Key, Price = p.Value });

            return (sort?.Trim().ToLowerInvariant()) switch
            {
                null or "" => items,
                "price" => items.OrderBy(i => i.Price),
                "-price" => items.OrderByDescending(i => i.Price),
                _ => throw new BadRequestException($"Невідоме значення sort='{sort}'. Допустимі значення: price, -price.")
            };
        }

        public async Task<TicketBookingResponseDto> UpdateAsync(string id, UpdateTicketBookingDto dto)
        {
            TicketBooking booking = await GetExistingAsync(id);
            _mapper.Map(dto, booking);

            await _bookings.UpdateAsync(booking);
            return _mapper.Map<TicketBookingResponseDto>(booking);
        }

        public async Task DeleteAsync(string id)
        {
            TicketBooking booking = await GetExistingAsync(id);

            if (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed)
                await _seats.UpdateStatusAsync(booking.SeatIds, SeatStatus.Free);

            await _bookings.DeleteAsync(id);
        }

        public async Task<TicketBookingResponseDto> ReserveAsync(ReserveBookingRequestDto dto)
        {
            await ReleaseExpiredBookingsAsync();

            MovieSession session = await _sessions.GetByIdAsync(dto.MovieSessionId)
                ?? throw new NotFoundException($"Сеанс з ID={dto.MovieSessionId} не знайдено.");

            if (session.Status != MovieSessionStatus.Scheduled)
                throw new BusinessRuleException($"Бронювання можливе лише на сеанс у статусі Scheduled. Поточний статус: {session.Status}.");

            if (session.StartTime < DateTime.Now.AddMinutes(BookingClosesMinutes))
                throw new BusinessRuleException($"Бронювання закривається за {BookingClosesMinutes} хвилин до початку сеансу.");

            List<string> seatIds = dto.SeatIds.Distinct().ToList();
            IReadOnlyList<CinemaHallSeat> seats = await _seats.GetByIdsAsync(seatIds);

            List<string> missing = seatIds.Except(seats.Select(s => s.Id)).ToList();
            if (missing.Count > 0)
                throw new NotFoundException($"Місця не знайдено: {string.Join(", ", missing)}.");

            if (seats.Any(s => s.MovieSessionId != session.Id))
                throw new BadRequestException("Усі місця мають належати обраному сеансу.");

            List<string> busySeats = seats.Where(s => s.Status != SeatStatus.Free)
                .Select(s => $"ряд {s.RowNumber} місце {s.SeatNumber}").ToList();
            if (busySeats.Count > 0)
                throw new BusinessRuleException($"Місця вже зайняті іншими глядачами: {string.Join(", ", busySeats)}.");

            TicketBooking booking = _mapper.Map<TicketBooking>(dto);
            booking.SeatIds = seatIds;
            booking.UserEmail = dto.UserEmail.Trim().ToLowerInvariant();
            foreach (BarItem item in booking.BarItems)
                item.Price = BarMenu.Prices[item.Product];
            booking.Status = BookingStatus.Pending;
            booking.CreatedAt = DateTime.Now;

            TicketBooking created = await _bookings.CreateAsync(booking);
            await _seats.UpdateStatusAsync(seatIds, SeatStatus.Reserved);

            return _mapper.Map<TicketBookingResponseDto>(created);
        }

        public async Task<TicketBookingResponseDto> PayAsync(string id)
        {
            TicketBooking booking = await GetExistingAsync(id);

            if (booking.Status == BookingStatus.Pending && IsHoldExpired(booking))
            {
                await ReleaseExpiredBookingsAsync();
                throw new BusinessRuleException($"Час на оплату ({HoldMinutes} хв) вичерпано, місця повернуто у продаж.");
            }

            if (booking.Status != BookingStatus.Pending)
                throw new BusinessRuleException($"Оплатити можна лише замовлення у статусі Pending. Поточний статус: {booking.Status}.");

            MovieSession session = await _sessions.GetByIdAsync(booking.MovieSessionId)
                ?? throw new NotFoundException($"Сеанс з ID={booking.MovieSessionId} не знайдено.");
            IReadOnlyList<CinemaHallSeat> seats = await _seats.GetByIdsAsync(booking.SeatIds);

            decimal ticketsPrice = 0;
            foreach (CinemaHallSeat seat in seats)
            {
                decimal price = session.BasePrice;
                if (seat.Category == SeatCategory.VIP)
                    price *= VipMultiplier;
                else if (session.StartTime.Hour < MorningShowEndHour)
                    price *= MorningStandardMultiplier;
                ticketsPrice += price;
            }

            booking.TicketsPrice = ticketsPrice;
            booking.BarDiscount = seats.Count >= FamilyTicketsCount
                ? Math.Round(booking.TotalBarPrice * FamilyBarDiscount, 2)
                : 0;
            booking.IsBarReceiptClosed = true;
            booking.Status = BookingStatus.Confirmed;
            booking.QrCode = Guid.NewGuid().ToString("N");
            booking.IsQrActive = true;

            await _bookings.UpdateAsync(booking);
            await _seats.UpdateStatusAsync(booking.SeatIds, SeatStatus.Sold);

            return _mapper.Map<TicketBookingResponseDto>(booking);
        }

        public async Task<TicketBookingResponseDto> RefundAsync(string id)
        {
            TicketBooking booking = await GetExistingAsync(id);

            if (booking.Status != BookingStatus.Confirmed)
                throw new BusinessRuleException($"Повернути можна лише оплачене замовлення (Confirmed). Поточний статус: {booking.Status}.");

            MovieSession session = await _sessions.GetByIdAsync(booking.MovieSessionId)
                ?? throw new NotFoundException($"Сеанс з ID={booking.MovieSessionId} не знайдено.");

            if (DateTime.Now > session.StartTime.AddMinutes(-RefundClosesMinutes))
                throw new BusinessRuleException($"Повернення квитків можливе не пізніше ніж за {RefundClosesMinutes} хвилин до початку показу.");

            decimal barTotal = booking.TotalBarPrice - booking.BarDiscount;
            booking.RefundAmount = booking.TicketsPrice + (booking.IsBarReceiptClosed ? 0 : barTotal);
            booking.Status = BookingStatus.Refunded;
            booking.IsQrActive = false;

            await _bookings.UpdateAsync(booking);
            await _seats.UpdateStatusAsync(booking.SeatIds, SeatStatus.Free);

            return _mapper.Map<TicketBookingResponseDto>(booking);
        }

        public async Task<CancelSessionResultDto> CancelSessionAsync(string movieSessionId)
        {
            MovieSession session = await _sessions.GetByIdAsync(movieSessionId)
                ?? throw new NotFoundException($"Сеанс з ID={movieSessionId} не знайдено.");

            if (session.Status != MovieSessionStatus.Scheduled)
                throw new BusinessRuleException($"Скасувати можна лише сеанс у статусі Scheduled. Поточний статус: {session.Status}.");

            session.Status = MovieSessionStatus.Cancelled;
            await _sessions.UpdateAsync(session);

            int refundedCount = 0;
            decimal refundedTotal = 0;
            foreach (TicketBooking booking in await _bookings.GetBySessionIdAsync(movieSessionId))
            {
                if (booking.Status == BookingStatus.Confirmed)
                {
                    booking.Status = BookingStatus.Refunded;
                    booking.RefundAmount = booking.TotalPrice;
                    booking.CompensationBonus = CompensationBonus;
                    booking.IsQrActive = false;
                    refundedCount++;
                    refundedTotal += booking.RefundAmount;
                }
                else if (booking.Status == BookingStatus.Pending)
                {
                    booking.Status = BookingStatus.Expired;
                }
                else
                {
                    continue;
                }

                await _bookings.UpdateAsync(booking);
            }

            await _seats.UpdateStatusBySessionAsync(movieSessionId, SeatStatus.Free);

            return new CancelSessionResultDto
            {
                Message = $"Сеанс '{session.MovieTitle}' скасовано адміністрацією.",
                RefundedBookings = refundedCount,
                RefundedTotal = refundedTotal,
                CompensationBonusPerBooking = CompensationBonus
            };
        }

        public async Task<BookingStatisticsDto> GetStatisticsAsync(string movieSessionId)
        {
            if (string.IsNullOrWhiteSpace(movieSessionId))
                throw new BadRequestException("Ідентифікатор сеансу є обов'язковим.");

            await ReleaseExpiredBookingsAsync();

            MovieSession session = await _sessions.GetByIdAsync(movieSessionId)
                ?? throw new NotFoundException($"Сеанс з ID={movieSessionId} не знайдено.");

            IReadOnlyList<TicketBooking> bookings = await _bookings.GetBySessionIdAsync(movieSessionId);
            List<TicketBooking> confirmed = bookings.Where(b => b.Status == BookingStatus.Confirmed).ToList();

            return new BookingStatisticsDto
            {
                MovieSessionId = session.Id,
                MovieTitle = session.MovieTitle,
                TotalBookings = bookings.Count,
                ConfirmedBookings = confirmed.Count,
                PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending),
                RefundedBookings = bookings.Count(b => b.Status == BookingStatus.Refunded),
                ExpiredBookings = bookings.Count(b => b.Status == BookingStatus.Expired),
                SoldSeats = confirmed.Sum(b => b.SeatIds.Count),
                TotalRevenue = confirmed.Sum(b => b.TotalPrice),
                AverageBookingPrice = confirmed.Count == 0 ? 0 : Math.Round(confirmed.Average(b => b.TotalPrice), 2),
                BarRevenue = confirmed.Sum(b => b.TotalBarPrice - b.BarDiscount),
                MostExpensiveBookingId = confirmed.OrderByDescending(b => b.TotalPrice).FirstOrDefault()?.Id,
                CheapestBookingId = confirmed.OrderBy(b => b.TotalPrice).FirstOrDefault()?.Id,
                TopBarProduct = confirmed
                    .SelectMany(b => b.BarItems)
                    .GroupBy(i => i.Product)
                    .OrderByDescending(g => g.Sum(i => i.Quantity))
                    .Select(g => (BarProduct?)g.Key)
                    .FirstOrDefault()
            };
        }

        public async Task<decimal> GetMonthlyRevenueAsync(int year, int month)
        {
            if (month < 1 || month > 12)
                throw new BadRequestException("Місяць має бути від 1 до 12.");

            if (year < MinReportYear || year > MaxReportYear)
                throw new BadRequestException($"Рік має бути від {MinReportYear} до {MaxReportYear}.");

            IReadOnlyList<TicketBooking> bookings = await _bookings.GetAllAsync();

            return bookings
                .Where(b => b.Status == BookingStatus.Confirmed && b.CreatedAt.Year == year && b.CreatedAt.Month == month)
                .Sum(b => b.TotalPrice);
        }

        private async Task ReleaseExpiredBookingsAsync()
        {
            IReadOnlyList<TicketBooking> expired = await _bookings.GetPendingCreatedBeforeAsync(DateTime.Now.AddMinutes(-HoldMinutes));

            foreach (TicketBooking booking in expired)
            {
                booking.Status = BookingStatus.Expired;
                await _bookings.UpdateAsync(booking);
                await _seats.UpdateStatusAsync(booking.SeatIds, SeatStatus.Free);
            }
        }

        private static bool IsHoldExpired(TicketBooking booking)
        {
            return booking.CreatedAt.AddMinutes(HoldMinutes) < DateTime.Now;
        }

        private async Task<TicketBooking> GetExistingAsync(string id)
        {
            return await _bookings.GetByIdAsync(id)
                ?? throw new NotFoundException($"Замовлення з ID={id} не знайдено.");
        }
    }
}

using AutoMapper;
using CinemaWebAPI.Dtos;
using CinemaWebAPI.Exceptions;
using CinemaWebAPI.Mapping;
using CinemaWebAPI.Models;
using CinemaWebAPI.Repositories;
using CinemaWebAPI.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CinemaWebAPI.UnitTests
{
    public class TicketBookingServiceTests
    {
        private const string SessionId = "665f1a2b3c4d5e6f7a8b9c01";
        private const string BookingId = "665f1a2b3c4d5e6f7a8b9c02";

        private readonly Mock<ITicketBookingRepository> _bookingsRepo = new();
        private readonly Mock<IMovieSessionRepository> _sessionsRepo = new();
        private readonly Mock<ICinemaHallSeatRepository> _seatsRepo = new();
        private readonly TicketBookingService _service;

        public TicketBookingServiceTests()
        {
            MapperConfiguration config = new MapperConfiguration(cfg => cfg.AddProfile<DtoMappingProfile>(), NullLoggerFactory.Instance);
            IMapper mapper = config.CreateMapper();

            _bookingsRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TicketBooking>());
            _bookingsRepo.Setup(r => r.GetBySessionIdAsync(It.IsAny<string>())).ReturnsAsync(new List<TicketBooking>());
            _bookingsRepo.Setup(r => r.GetPendingCreatedBeforeAsync(It.IsAny<DateTime>())).ReturnsAsync(new List<TicketBooking>());
            _bookingsRepo.Setup(r => r.UpdateAsync(It.IsAny<TicketBooking>())).ReturnsAsync(true);
            _bookingsRepo.Setup(r => r.DeleteAsync(It.IsAny<string>())).ReturnsAsync(true);
            _bookingsRepo.Setup(r => r.CreateAsync(It.IsAny<TicketBooking>()))
                .ReturnsAsync((TicketBooking b) =>
                {
                    b.Id = BookingId;
                    return b;
                });
            _sessionsRepo.Setup(r => r.UpdateAsync(It.IsAny<MovieSession>())).ReturnsAsync(true);

            _service = new TicketBookingService(_bookingsRepo.Object, _sessionsRepo.Object, _seatsRepo.Object, mapper);
        }

        private static MovieSession NewSession(MovieSessionStatus status = MovieSessionStatus.Scheduled, DateTime? start = null, decimal basePrice = 200)
        {
            return new MovieSession
            {
                Id = SessionId,
                MovieTitle = "Дюна: Частина друга",
                BasePrice = basePrice,
                HallNumber = 1,
                DurationMinutes = 166,
                Status = status,
                StartTime = start ?? DateTime.Now.AddHours(3)
            };
        }

        private static CinemaHallSeat NewSeat(string id, SeatCategory category = SeatCategory.Standard, SeatStatus status = SeatStatus.Free, string sessionId = SessionId)
        {
            return new CinemaHallSeat
            {
                Id = id,
                MovieSessionId = sessionId,
                RowNumber = 1,
                SeatNumber = int.Parse(id.Substring(1)),
                Category = category,
                Status = status
            };
        }

        private static TicketBooking NewBooking(string id, BookingStatus status, decimal ticketsPrice = 0, int seats = 2,
            DateTime? createdAt = null, params (BarProduct Product, int Quantity)[] bar)
        {
            return new TicketBooking
            {
                Id = id,
                MovieSessionId = SessionId,
                SeatIds = Enumerable.Range(1, seats).Select(i => $"s{i}").ToList(),
                Status = status,
                CreatedAt = createdAt ?? DateTime.Now,
                UserEmail = "viewer@cinema.ua",
                TicketsPrice = ticketsPrice,
                BarItems = bar.Select(b => new BarItem { Product = b.Product, Quantity = b.Quantity, Price = BarMenu.Prices[b.Product] }).ToList()
            };
        }

        private static ReserveBookingRequestDto NewReserveRequest(params string[] seatIds)
        {
            return new ReserveBookingRequestDto
            {
                MovieSessionId = SessionId,
                SeatIds = seatIds.ToList(),
                UserEmail = "  Viewer@Cinema.UA ",
                BarItems = new List<BarItemRequestDto> { new BarItemRequestDto { Product = BarProduct.ComboClassic, Quantity = 1 } }
            };
        }

        private void SetupSession(MovieSession? session)
        {
            _sessionsRepo.Setup(r => r.GetByIdAsync(SessionId)).ReturnsAsync(session);
        }

        private void SetupSeats(params CinemaHallSeat[] seats)
        {
            _seatsRepo.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(seats.ToList());
        }

        private void SetupBooking(TicketBooking? booking)
        {
            _bookingsRepo.Setup(r => r.GetByIdAsync(BookingId)).ReturnsAsync(booking);
        }

        private static bool SameIds(IEnumerable<string> actual, params string[] expected)
        {
            return actual.OrderBy(x => x).SequenceEqual(expected.OrderBy(x => x));
        }

        [Fact]
        public async Task ReserveAsync_ValidRequest_CreatesPendingBookingAndReservesSeats()
        {
            SetupSession(NewSession());
            SetupSeats(NewSeat("s1"), NewSeat("s2"));

            TicketBookingResponseDto result = await _service.ReserveAsync(NewReserveRequest("s1", "s2"));

            Assert.Equal(BookingId, result.Id);
            Assert.Equal(BookingStatus.Pending, result.Status);
            Assert.Equal("viewer@cinema.ua", result.UserEmail);
            Assert.Equal(200, result.TotalBarPrice);
            Assert.Equal(BarMenu.Prices[BarProduct.ComboClassic], result.BarItems.Single().Price);
            _bookingsRepo.Verify(r => r.CreateAsync(It.Is<TicketBooking>(b => b.Status == BookingStatus.Pending)), Times.Once);
            _seatsRepo.Verify(r => r.UpdateStatusAsync(It.Is<IEnumerable<string>>(ids => SameIds(ids, "s1", "s2")), SeatStatus.Reserved), Times.Once);
        }

        [Fact]
        public async Task ReserveAsync_SessionNotFound_ThrowsNotFoundException()
        {
            SetupSession(null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.ReserveAsync(NewReserveRequest("s1")));

            _bookingsRepo.Verify(r => r.CreateAsync(It.IsAny<TicketBooking>()), Times.Never);
            _seatsRepo.Verify(r => r.UpdateStatusAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<SeatStatus>()), Times.Never);
        }

        [Theory]
        [InlineData(SeatStatus.Reserved)]
        [InlineData(SeatStatus.Sold)]
        public async Task ReserveAsync_SeatAlreadyTaken_ThrowsBusinessRuleException(SeatStatus status)
        {
            SetupSession(NewSession());
            SetupSeats(NewSeat("s1"), NewSeat("s2", status: status));

            BusinessRuleException ex = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ReserveAsync(NewReserveRequest("s1", "s2")));

            Assert.Contains("місце 2", ex.Message);
            _seatsRepo.Verify(r => r.UpdateStatusAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<SeatStatus>()), Times.Never);
        }

        [Fact]
        public async Task PayAsync_VipSeat_AppliesVipMultiplier()
        {
            SetupBooking(NewBooking(BookingId, BookingStatus.Pending, seats: 1));
            SetupSession(NewSession(start: new DateTime(2030, 1, 1, 19, 0, 0)));
            SetupSeats(NewSeat("s1", SeatCategory.VIP));

            TicketBookingResponseDto result = await _service.PayAsync(BookingId);

            Assert.Equal(300, result.TicketsPrice); 
        }

        [Fact]
        public async Task PayAsync_ValidBooking_ConfirmsBookingAndSellsSeats()
        {
            TicketBooking booking = NewBooking(BookingId, BookingStatus.Pending);
            SetupBooking(booking);
            SetupSession(NewSession());
            SetupSeats(NewSeat("s1"), NewSeat("s2"));

            TicketBookingResponseDto result = await _service.PayAsync(BookingId);

            Assert.True(result.IsQrActive);
            Assert.True(result.IsBarReceiptClosed);
            Assert.NotNull(result.QrCode);
            Assert.Equal(32, result.QrCode!.Length);
            _bookingsRepo.Verify(r => r.UpdateAsync(booking), Times.Once);
            _seatsRepo.Verify(r => r.UpdateStatusAsync(It.Is<IEnumerable<string>>(ids => SameIds(ids, "s1", "s2")), SeatStatus.Sold), Times.Once);
        }

        [Fact]
        public async Task PayAsync_BookingNotFound_ThrowsNotFoundException()
        {
            SetupBooking(null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.PayAsync(BookingId));

            _sessionsRepo.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RefundAsync_OpenBarReceipt_RefundsTicketsAndBar()
        {
            TicketBooking booking = NewBooking(BookingId, BookingStatus.Confirmed, ticketsPrice: 400, bar: (BarProduct.Popcorn, 1));
            booking.IsBarReceiptClosed = false;
            SetupBooking(booking);
            SetupSession(NewSession(start: DateTime.Now.AddHours(2)));

            TicketBookingResponseDto result = await _service.RefundAsync(BookingId);

            Assert.Equal(520, result.RefundAmount);
        }

        [Fact]
        public async Task RefundAsync_LessThan30MinutesBeforeStart_ThrowsBusinessRuleException()
        {
            SetupBooking(NewBooking(BookingId, BookingStatus.Confirmed, ticketsPrice: 400));
            SetupSession(NewSession(start: DateTime.Now.AddMinutes(20)));

            await Assert.ThrowsAsync<BusinessRuleException>(() => _service.RefundAsync(BookingId));

            _bookingsRepo.Verify(r => r.UpdateAsync(It.IsAny<TicketBooking>()), Times.Never);
            _seatsRepo.Verify(r => r.UpdateStatusAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<SeatStatus>()), Times.Never);
        }

        [Fact]
        public async Task GetStatisticsAsync_MixedBookings_CalculatesAggregates()
        {
            TicketBooking big = NewBooking("b1", BookingStatus.Confirmed, ticketsPrice: 800, seats: 4, bar: (BarProduct.Popcorn, 3));
            big.BarDiscount = 54;
            TicketBooking small = NewBooking("b2", BookingStatus.Confirmed, ticketsPrice: 200, seats: 1, bar: new[] { (BarProduct.Popcorn, 1), (BarProduct.Cola, 2) });
            TicketBooking pending = NewBooking("b3", BookingStatus.Pending, seats: 2);
            TicketBooking refunded = NewBooking("b4", BookingStatus.Refunded, ticketsPrice: 999, seats: 3);
            TicketBooking expired = NewBooking("b5", BookingStatus.Expired, seats: 1);
            SetupSession(NewSession());
            _bookingsRepo.Setup(r => r.GetBySessionIdAsync(SessionId))
                .ReturnsAsync(new List<TicketBooking> { big, small, pending, refunded, expired });

            BookingStatisticsDto result = await _service.GetStatisticsAsync(SessionId);

            Assert.Equal(SessionId, result.MovieSessionId);
            Assert.Equal("Дюна: Частина друга", result.MovieTitle);
            Assert.Equal(5, result.TotalBookings);
            Assert.Equal(2, result.ConfirmedBookings);
            Assert.Equal(1, result.PendingBookings);
            Assert.Equal(1, result.RefundedBookings);
            Assert.Equal(1, result.ExpiredBookings);
            Assert.Equal(5, result.SoldSeats);
            Assert.Equal(1566, result.TotalRevenue);
            Assert.Equal(783, result.AverageBookingPrice);
            Assert.Equal(566, result.BarRevenue);
            Assert.Equal("b1", result.MostExpensiveBookingId);
            Assert.Equal("b2", result.CheapestBookingId);
            Assert.Equal(BarProduct.Popcorn, result.TopBarProduct);
            _bookingsRepo.Verify(r => r.GetBySessionIdAsync(SessionId), Times.Once);
            _sessionsRepo.Verify(r => r.GetByIdAsync(SessionId), Times.Once);
        }

        [Fact]
        public async Task GetStatisticsAsync_NoBookings_ReturnsZeroValues()
        {
            SetupSession(NewSession());

            BookingStatisticsDto result = await _service.GetStatisticsAsync(SessionId);

            Assert.Equal(0, result.TotalBookings);
            Assert.Equal(0, result.SoldSeats);
            Assert.Equal(0, result.TotalRevenue);
            Assert.Equal(0, result.AverageBookingPrice);
            Assert.Null(result.MostExpensiveBookingId);
            Assert.Null(result.CheapestBookingId);
            Assert.Null(result.TopBarProduct);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetStatisticsAsync_BlankSessionId_ThrowsBadRequestException(string? sessionId)
        {
            await Assert.ThrowsAsync<BadRequestException>(() => _service.GetStatisticsAsync(sessionId!));

            _sessionsRepo.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
            _bookingsRepo.Verify(r => r.GetBySessionIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetStatisticsAsync_SessionNotFound_ThrowsNotFoundException()
        {
            SetupSession(null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetStatisticsAsync(SessionId));

            _bookingsRepo.Verify(r => r.GetBySessionIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetMonthlyRevenueAsync_BookingsInDifferentMonths_SumsOnlyConfirmedOfRequestedMonth()
        {
            _bookingsRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TicketBooking>
            {
                NewBooking("b1", BookingStatus.Confirmed, ticketsPrice: 400, createdAt: new DateTime(2026, 11, 3)),
                NewBooking("b2", BookingStatus.Confirmed, ticketsPrice: 250, createdAt: new DateTime(2026, 11, 28), bar: (BarProduct.Cola, 2)),
                NewBooking("b3", BookingStatus.Confirmed, ticketsPrice: 900, createdAt: new DateTime(2026, 10, 31)),
                NewBooking("b4", BookingStatus.Confirmed, ticketsPrice: 700, createdAt: new DateTime(2025, 11, 10)),
                NewBooking("b5", BookingStatus.Refunded, ticketsPrice: 500, createdAt: new DateTime(2026, 11, 15)),
                NewBooking("b6", BookingStatus.Pending, ticketsPrice: 300, createdAt: new DateTime(2026, 11, 20))
            });

            decimal result = await _service.GetMonthlyRevenueAsync(2026, 11);

            Assert.Equal(400 + 250 + 140, result);
            _bookingsRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetMonthlyRevenueAsync_NoBookingsInMonth_ReturnsZero()
        {
            decimal result = await _service.GetMonthlyRevenueAsync(2026, 5);

            Assert.Equal(0, result);
        }

        [Theory]
        [InlineData(2026, 0)]
        [InlineData(2026, 13)]
        public async Task GetMonthlyRevenueAsync_InvalidMonth_ThrowsBadRequestException(int year, int month)
        {
            BadRequestException ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.GetMonthlyRevenueAsync(year, month));

            Assert.Contains("Місяць", ex.Message);
            _bookingsRepo.Verify(r => r.GetAllAsync(), Times.Never);
        }

        [Theory]
        [InlineData(1999, 6)]
        [InlineData(2101, 6)]
        public async Task GetMonthlyRevenueAsync_InvalidYear_ThrowsBadRequestException(int year, int month)
        {
            BadRequestException ex = await Assert.ThrowsAsync<BadRequestException>(() => _service.GetMonthlyRevenueAsync(year, month));

            Assert.Contains("Рік", ex.Message);
            _bookingsRepo.Verify(r => r.GetAllAsync(), Times.Never);
        }
    }
}

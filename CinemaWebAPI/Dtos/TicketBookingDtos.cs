using CinemaWebAPI.Models;

namespace CinemaWebAPI.Dtos
{
    public class BarItemRequestDto
    {
        public BarProduct Product { get; set; }
        public int Quantity { get; set; }
    }

    public class BarItemResponseDto
    {
        public BarProduct Product { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class BarMenuItemDto
    {
        public BarProduct Product { get; set; }
        public decimal Price { get; set; }
    }

    public class ReserveBookingRequestDto
    {
        public string MovieSessionId { get; set; } = "";
        public List<string> SeatIds { get; set; } = new List<string>();
        public string UserEmail { get; set; } = "";
        public List<BarItemRequestDto> BarItems { get; set; } = new List<BarItemRequestDto>();
    }

    public class UpdateTicketBookingDto
    {
        public string UserEmail { get; set; } = "";
    }

    public class TicketBookingResponseDto
    {
        public string Id { get; set; } = "";
        public string MovieSessionId { get; set; } = "";
        public List<string> SeatIds { get; set; } = new List<string>();
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserEmail { get; set; } = "";
        public List<BarItemResponseDto> BarItems { get; set; } = new List<BarItemResponseDto>();
        public string? QrCode { get; set; }
        public bool IsQrActive { get; set; }
        public decimal TicketsPrice { get; set; }
        public decimal TotalBarPrice { get; set; }
        public decimal BarDiscount { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsBarReceiptClosed { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal CompensationBonus { get; set; }
    }

    public class BookingStatisticsDto
    {
        public string MovieSessionId { get; set; } = "";
        public string MovieTitle { get; set; } = "";
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int PendingBookings { get; set; }
        public int RefundedBookings { get; set; }
        public int ExpiredBookings { get; set; }
        public int SoldSeats { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageBookingPrice { get; set; }
        public decimal BarRevenue { get; set; }
        public string? MostExpensiveBookingId { get; set; }
        public string? CheapestBookingId { get; set; }
        public BarProduct? TopBarProduct { get; set; }
    }

    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CancelSessionResultDto
    {
        public string Message { get; set; } = "";
        public int RefundedBookings { get; set; }
        public decimal RefundedTotal { get; set; }
        public decimal CompensationBonusPerBooking { get; set; }
    }
}

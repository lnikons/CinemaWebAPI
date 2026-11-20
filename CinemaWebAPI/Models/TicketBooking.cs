namespace CinemaWebAPI.Models
{
    public enum BarProduct
    {
        Popcorn,
        Nachos,
        Cola,
        ComboClassic
    }

    public static class BarMenu
    {
        public static readonly Dictionary<BarProduct, decimal> Prices = new()
        {
            { BarProduct.Popcorn, 120 },
            { BarProduct.Cola, 70 },
            { BarProduct.Nachos, 100 },
            { BarProduct.ComboClassic, 200 },
        };
    }

    public class BarItem
    {
        public BarProduct Product { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Refunded,
        Expired
    }

    public class TicketBooking : IEntity
    {
        public string Id { get; set; } = "";
        public string MovieSessionId { get; set; } = "";
        public List<string> SeatIds { get; set; } = new List<string>();
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserEmail { get; set; } = "";
        public List<BarItem> BarItems { get; set; } = new List<BarItem>();
        public string? QrCode { get; set; }
        public bool IsQrActive { get; set; }
        public decimal TicketsPrice { get; set; }
        public decimal BarDiscount { get; set; }
        public bool IsBarReceiptClosed { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal CompensationBonus { get; set; }

        public decimal TotalBarPrice => BarItems.Sum(i => i.Price * i.Quantity);
        public decimal TotalPrice => TicketsPrice + TotalBarPrice - BarDiscount;
    }
}

namespace CinemaWebAPI.Models
{
    public enum SeatCategory
    {
        Standard,
        VIP
    }

    public enum SeatStatus
    {
        Free,
        Sold,
        Reserved
    }

    public class CinemaHallSeat : IEntity
    {
        public string Id { get; set; } = "";
        public string MovieSessionId { get; set; } = "";
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
        public SeatCategory Category { get; set; }
        public SeatStatus Status { get; set; }
    }
}

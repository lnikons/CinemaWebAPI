namespace CinemaWebAPI.Models
{
    public enum MovieSessionStatus
    {
        Scheduled,
        Ongoing,
        Completed,
        Cancelled
    }

    public class MovieSession : IEntity
    {
        public int MinAge { get; set; }
        public string Id { get; set; } = "";
        public string MovieTitle { get; set; } = "";
        public decimal BasePrice { get; set; }
        public int HallNumber { get; set; }
        public int DurationMinutes { get; set; }
        public MovieSessionStatus Status { get; set; }
        public DateTime StartTime { get; set; }
    }
}

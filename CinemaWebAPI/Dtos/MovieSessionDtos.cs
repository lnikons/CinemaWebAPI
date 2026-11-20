using CinemaWebAPI.Models;

namespace CinemaWebAPI.Dtos
{
    public class CreateMovieSessionDto
    {
        public int MinAge { get; set; }
        public string MovieTitle { get; set; } = "";
        public decimal BasePrice { get; set; }
        public int HallNumber { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class UpdateMovieSessionDto
    {
        public int MinAge { get; set; }
        public string MovieTitle { get; set; } = "";
        public decimal BasePrice { get; set; }
        public int HallNumber { get; set; }
        public int DurationMinutes { get; set; }
        public MovieSessionStatus Status { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class PatchMovieSessionDto
    {
        public int? MinAge { get; set; }
        public string? MovieTitle { get; set; }
        public decimal? BasePrice { get; set; }
        public int? HallNumber { get; set; }
        public int? DurationMinutes { get; set; }
        public MovieSessionStatus? Status { get; set; }
        public DateTime? StartTime { get; set; }
    }

    public class MovieSessionResponseDto
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

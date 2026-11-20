using CinemaWebAPI.Models;

namespace CinemaWebAPI.Dtos
{
    public class CreateCinemaHallSeatDto
    {
        public string MovieSessionId { get; set; } = "";
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
        public SeatCategory Category { get; set; }
    }

    public class UpdateCinemaHallSeatDto
    {
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
        public SeatCategory Category { get; set; }
        public SeatStatus Status { get; set; }
    }

    public class PatchCinemaHallSeatDto
    {
        public int? RowNumber { get; set; }
        public int? SeatNumber { get; set; }
        public SeatCategory? Category { get; set; }
        public SeatStatus? Status { get; set; }
    }

    public class CinemaHallSeatResponseDto
    {
        public string Id { get; set; } = "";
        public string MovieSessionId { get; set; } = "";
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
        public SeatCategory Category { get; set; }
        public SeatStatus Status { get; set; }
    }
}

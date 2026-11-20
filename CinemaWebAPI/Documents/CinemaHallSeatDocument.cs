using CinemaWebAPI.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CinemaWebAPI.Documents
{
    public class CinemaHallSeatDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        [BsonElement("movieSessionId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MovieSessionId { get; set; } = "";

        [BsonElement("rowNumber")]
        public int RowNumber { get; set; }

        [BsonElement("seatNumber")]
        public int SeatNumber { get; set; }

        [BsonElement("category")]
        [BsonRepresentation(BsonType.String)]
        public SeatCategory Category { get; set; }

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public SeatStatus Status { get; set; }
    }
}

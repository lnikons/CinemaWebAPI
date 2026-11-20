using CinemaWebAPI.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CinemaWebAPI.Documents
{
    public class MovieSessionDocument
    {
        [BsonElement("minAge")]
        public int MinAge { get; set; }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        [BsonElement("movieTitle")]
        public string MovieTitle { get; set; } = "";

        [BsonElement("basePrice")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal BasePrice { get; set; }

        [BsonElement("hallNumber")]
        public int HallNumber { get; set; }

        [BsonElement("durationMinutes")]
        public int DurationMinutes { get; set; }

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public MovieSessionStatus Status { get; set; }

        [BsonElement("startTime")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartTime { get; set; }
    }
}

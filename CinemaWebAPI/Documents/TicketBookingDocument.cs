using CinemaWebAPI.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CinemaWebAPI.Documents
{
    public class BarItemDocument
    {
        [BsonElement("product")]
        [BsonRepresentation(BsonType.String)]
        public BarProduct Product { get; set; }

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }
    }

    public class TicketBookingDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        [BsonElement("movieSessionId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MovieSessionId { get; set; } = "";

        [BsonElement("seatIds")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> SeatIds { get; set; } = new List<string>();

        [BsonElement("status")]
        [BsonRepresentation(BsonType.String)]
        public BookingStatus Status { get; set; }

        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedAt { get; set; }

        [BsonElement("userEmail")]
        public string UserEmail { get; set; } = "";

        [BsonElement("barItems")]
        public List<BarItemDocument> BarItems { get; set; } = new List<BarItemDocument>();

        [BsonElement("qrCode")]
        public string? QrCode { get; set; }

        [BsonElement("isQrActive")]
        public bool IsQrActive { get; set; }

        [BsonElement("ticketsPrice")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal TicketsPrice { get; set; }

        [BsonElement("barDiscount")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal BarDiscount { get; set; }

        [BsonElement("isBarReceiptClosed")]
        public bool IsBarReceiptClosed { get; set; }

        [BsonElement("refundAmount")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal RefundAmount { get; set; }

        [BsonElement("compensationBonus")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal CompensationBonus { get; set; }
    }
}

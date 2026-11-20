using AutoMapper;
using CinemaWebAPI.Documents;
using CinemaWebAPI.Models;
using MongoDB.Driver;

namespace CinemaWebAPI.Repositories
{
    public interface ITicketBookingRepository : IRepository<TicketBooking>
    {
        Task<IReadOnlyList<TicketBooking>> GetBySessionIdAsync(string movieSessionId);
        Task<IReadOnlyList<TicketBooking>> GetPendingCreatedBeforeAsync(DateTime createdBefore);
    }

    public class TicketBookingRepository : MongoRepository<TicketBooking, TicketBookingDocument>, ITicketBookingRepository
    {
        public const string CollectionName = "ticketBookings";

        public TicketBookingRepository(IMongoDatabase database, IMapper mapper)
            : base(database, mapper, CollectionName)
        {
        }

        public async Task<IReadOnlyList<TicketBooking>> GetBySessionIdAsync(string movieSessionId)
        {
            if (!IsObjectId(movieSessionId))
                return new List<TicketBooking>();

            List<TicketBookingDocument> documents = await Collection.Find(b => b.MovieSessionId == movieSessionId).ToListAsync();
            return Mapper.Map<List<TicketBooking>>(documents);
        }

        public async Task<IReadOnlyList<TicketBooking>> GetPendingCreatedBeforeAsync(DateTime createdBefore)
        {
            List<TicketBookingDocument> documents = await Collection
                .Find(b => b.Status == BookingStatus.Pending && b.CreatedAt < createdBefore)
                .ToListAsync();
            return Mapper.Map<List<TicketBooking>>(documents);
        }
    }
}

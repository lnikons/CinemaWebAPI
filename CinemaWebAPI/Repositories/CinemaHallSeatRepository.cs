using AutoMapper;
using CinemaWebAPI.Documents;
using CinemaWebAPI.Models;
using MongoDB.Driver;

namespace CinemaWebAPI.Repositories
{
    public interface ICinemaHallSeatRepository : IRepository<CinemaHallSeat>
    {
        Task<IReadOnlyList<CinemaHallSeat>> GetByIdsAsync(IEnumerable<string> ids);
        Task<IReadOnlyList<CinemaHallSeat>> GetBySessionIdAsync(string movieSessionId);
        Task<bool> ExistsAsync(string movieSessionId, int rowNumber, int seatNumber, string? exceptId);
        Task UpdateStatusAsync(IEnumerable<string> ids, SeatStatus status);
        Task UpdateStatusBySessionAsync(string movieSessionId, SeatStatus status);
    }

    public class CinemaHallSeatRepository : MongoRepository<CinemaHallSeat, CinemaHallSeatDocument>, ICinemaHallSeatRepository
    {
        public const string CollectionName = "cinemaHallSeats";

        public CinemaHallSeatRepository(IMongoDatabase database, IMapper mapper)
            : base(database, mapper, CollectionName)
        {
        }

        public async Task<IReadOnlyList<CinemaHallSeat>> GetByIdsAsync(IEnumerable<string> ids)
        {
            List<CinemaHallSeatDocument> documents = await Collection.Find(ByIds(ids)).ToListAsync();
            return Mapper.Map<List<CinemaHallSeat>>(documents);
        }

        public async Task<IReadOnlyList<CinemaHallSeat>> GetBySessionIdAsync(string movieSessionId)
        {
            if (!IsObjectId(movieSessionId))
                return new List<CinemaHallSeat>();

            List<CinemaHallSeatDocument> documents = await Collection
                .Find(s => s.MovieSessionId == movieSessionId)
                .SortBy(s => s.RowNumber).ThenBy(s => s.SeatNumber)
                .ToListAsync();
            return Mapper.Map<List<CinemaHallSeat>>(documents);
        }

        public async Task<bool> ExistsAsync(string movieSessionId, int rowNumber, int seatNumber, string? exceptId)
        {
            FilterDefinitionBuilder<CinemaHallSeatDocument> filter = Builders<CinemaHallSeatDocument>.Filter;
            FilterDefinition<CinemaHallSeatDocument> query = filter.Eq(s => s.MovieSessionId, movieSessionId)
                & filter.Eq(s => s.RowNumber, rowNumber)
                & filter.Eq(s => s.SeatNumber, seatNumber);
            if (exceptId != null)
                query &= filter.Ne(s => s.Id, exceptId);

            return await Collection.Find(query).AnyAsync();
        }

        public async Task UpdateStatusAsync(IEnumerable<string> ids, SeatStatus status)
        {
            await Collection.UpdateManyAsync(ByIds(ids), Builders<CinemaHallSeatDocument>.Update.Set(s => s.Status, status));
        }

        public async Task UpdateStatusBySessionAsync(string movieSessionId, SeatStatus status)
        {
            await Collection.UpdateManyAsync(s => s.MovieSessionId == movieSessionId,
                Builders<CinemaHallSeatDocument>.Update.Set(s => s.Status, status));
        }
    }
}

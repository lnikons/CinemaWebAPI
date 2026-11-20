using AutoMapper;
using CinemaWebAPI.Documents;
using CinemaWebAPI.Models;
using MongoDB.Driver;

namespace CinemaWebAPI.Repositories
{
    public interface IMovieSessionRepository : IRepository<MovieSession>
    {
    }

    public class MovieSessionRepository : MongoRepository<MovieSession, MovieSessionDocument>, IMovieSessionRepository
    {
        public const string CollectionName = "movieSessions";

        public MovieSessionRepository(IMongoDatabase database, IMapper mapper)
            : base(database, mapper, CollectionName)
        {
        }
    }
}

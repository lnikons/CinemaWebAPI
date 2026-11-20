using AutoMapper;
using CinemaWebAPI.Documents;
using CinemaWebAPI.Models;
using MongoDB.Driver;

namespace CinemaWebAPI.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsWithRoleAsync(UserRole role);
    }

    public class UserRepository : MongoRepository<User, UserDocument>, IUserRepository
    {
        public const string CollectionName = "users";

        public UserRepository(IMongoDatabase database, IMapper mapper)
            : base(database, mapper, CollectionName)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            string normalized = email.Trim().ToLowerInvariant();
            UserDocument? document = await Collection.Find(u => u.Email == normalized).FirstOrDefaultAsync();
            return document == null ? null : Mapper.Map<User>(document);
        }

        public async Task<bool> ExistsWithRoleAsync(UserRole role)
        {
            return await Collection.Find(u => u.Role == role).AnyAsync();
        }
    }
}

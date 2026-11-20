using AutoMapper;
using CinemaWebAPI.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CinemaWebAPI.Repositories
{
    public abstract class MongoRepository<TModel, TDocument> : IRepository<TModel>
        where TModel : class, IEntity
    {
        protected readonly IMongoCollection<TDocument> Collection;
        protected readonly IMapper Mapper;

        protected MongoRepository(IMongoDatabase database, IMapper mapper, string collectionName)
        {
            Collection = database.GetCollection<TDocument>(collectionName);
            Mapper = mapper;
        }

        public async Task<IReadOnlyList<TModel>> GetAllAsync()
        {
            List<TDocument> documents = await Collection.Find(FilterDefinition<TDocument>.Empty).ToListAsync();
            return Mapper.Map<List<TModel>>(documents);
        }

        public async Task<TModel?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return null;

            TDocument? document = await Collection.Find(ById(objectId)).FirstOrDefaultAsync();
            return document == null ? null : Mapper.Map<TModel>(document);
        }

        public async Task<TModel> CreateAsync(TModel entity)
        {
            TDocument document = Mapper.Map<TDocument>(entity);
            await Collection.InsertOneAsync(document);
            return Mapper.Map<TModel>(document);
        }

        public async Task<bool> UpdateAsync(TModel entity)
        {
            if (!ObjectId.TryParse(entity.Id, out ObjectId objectId))
                return false;

            ReplaceOneResult result = await Collection.ReplaceOneAsync(ById(objectId), Mapper.Map<TDocument>(entity));
            return result.MatchedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return false;

            DeleteResult result = await Collection.DeleteOneAsync(ById(objectId));
            return result.DeletedCount > 0;
        }

        protected static FilterDefinition<TDocument> ById(ObjectId id)
        {
            return Builders<TDocument>.Filter.Eq("_id", id);
        }

        protected static FilterDefinition<TDocument> ByIds(IEnumerable<string> ids)
        {
            List<ObjectId> objectIds = new List<ObjectId>();
            foreach (string id in ids)
            {
                if (ObjectId.TryParse(id, out ObjectId objectId))
                    objectIds.Add(objectId);
            }

            return Builders<TDocument>.Filter.In("_id", objectIds);
        }

        protected static bool IsObjectId(string id)
        {
            return ObjectId.TryParse(id, out _);
        }
    }
}

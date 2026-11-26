//using CinemaWebAPI.IntegrationTests.Infrastructure;
//using MongoDB.Bson;
//using MongoDB.Driver;
//
//namespace CinemaWebAPI.IntegrationTests.Controllers
//{
//    public class TestDatabaseTests : IntegrationTestBase
//    {
//        public TestDatabaseTests(ApiFactory factory) : base(factory) { }
//
//        private async Task<long> CountAsync(string collectionName)
//        {
//            IMongoCollection<BsonDocument> collection = Factory.Database.GetCollection<BsonDocument>(collectionName);
//            return await collection.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty);
//        }
//
//        [Fact]
//        public void Database_UsesTemporaryMongo2GoInstance_NotWorkingDatabase()
//        {
//            IMongoDatabase database = Factory.Database;
//            MongoServerAddress server = database.Client.Settings.Server;
//
//            Assert.Equal(ApiFactory.DatabaseName, database.DatabaseNamespace.DatabaseName);
//            Assert.NotEqual(27017, server.Port);
//        }
//
//        [Fact]
//        public async Task Database_AfterReset_ContainsOnlyFixedSeedData()
//        {
//            List<string> collections = (await Factory.Database.ListCollectionNamesAsync()).ToList();
//
//            Assert.Contains("movieSessions", collections);
//            Assert.Contains("cinemaHallSeats", collections);
//            Assert.Contains("users", collections);
//            Assert.Equal(4, await CountAsync("movieSessions"));
//            Assert.Equal(4, await CountAsync("cinemaHallSeats"));
//            Assert.Equal(2, await CountAsync("users"));
//            Assert.Equal(0, await CountAsync("ticketBookings"));
//        }
//    }
//}

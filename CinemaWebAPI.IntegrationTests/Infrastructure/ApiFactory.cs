//using CinemaWebAPI.Models;
//using CinemaWebAPI.Repositories;
//using CinemaWebAPI.Services;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.Extensions.DependencyInjection;
//using Mongo2Go;
//using MongoDB.Driver;
//
//namespace CinemaWebAPI.IntegrationTests.Infrastructure
//{
//    public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
//    {
//        public const string AdminEmail = "admin@cinema.ua";
//        public const string AdminPassword = "Admin12345";
//        public const string UserEmail = "viewer@cinema.ua";
//        public const string UserPassword = "Viewer12345";
//        public const string DatabaseName = "cinema_integration_tests";
//
//        public const string ScheduledSessionId = "665f1a2b3c4d5e6f7a8b9c01";
//        public const string SecondScheduledSessionId = "665f1a2b3c4d5e6f7a8b9c02";
//        public const string CompletedSessionId = "665f1a2b3c4d5e6f7a8b9c03";
//        public const string CancelledSessionId = "665f1a2b3c4d5e6f7a8b9c04";
//        public const string MissingId = "665f1a2b3c4d5e6f7a8b9cff";
//
//        public static readonly string[] SeatIds =
//        {
//            "665f1a2b3c4d5e6f7a8b9d01",
//            "665f1a2b3c4d5e6f7a8b9d02",
//            "665f1a2b3c4d5e6f7a8b9d03",
//            "665f1a2b3c4d5e6f7a8b9d04"
//        };
//
//        private MongoDbRunner? _runner;
//
//        public IMongoDatabase Database => Services.GetRequiredService<IMongoDatabase>();
//
//        protected override void ConfigureWebHost(IWebHostBuilder builder)
//        {
//            _runner ??= MongoDbRunner.Start();
//
//            builder.UseSetting("MongoDb:ConnectionString", _runner.ConnectionString);
//            builder.UseSetting("MongoDb:DatabaseName", DatabaseName);
//        }
//
//        public async Task InitializeAsync()
//        {
//            await ResetDatabaseAsync();
//        }
//
//        async Task IAsyncLifetime.DisposeAsync()
//        {
//            await base.DisposeAsync();
//            _runner?.Dispose();
//        }
//
//        public async Task ResetDatabaseAsync()
//        {
//            using IServiceScope scope = Services.CreateScope();
//            IMongoDatabase database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
//            IMovieSessionRepository sessions = scope.ServiceProvider.GetRequiredService<IMovieSessionRepository>();
//            ICinemaHallSeatRepository seats = scope.ServiceProvider.GetRequiredService<ICinemaHallSeatRepository>();
//            IUserRepository users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
//
//            foreach (string name in new[]
//            {
//                MovieSessionRepository.CollectionName,
//                CinemaHallSeatRepository.CollectionName,
//                TicketBookingRepository.CollectionName,
//                UserRepository.CollectionName
//            })
//            {
//                await database.DropCollectionAsync(name);
//            }
//
//            await users.CreateAsync(new User
//            {
//                Email = AdminEmail,
//                FullName = "Адміністратор кінотеатру",
//                PasswordHash = PasswordHasher.Hash(AdminPassword),
//                Role = UserRole.Admin,
//                CreatedAt = DateTime.UtcNow
//            });
//            await users.CreateAsync(new User
//            {
//                Email = UserEmail,
//                FullName = "Глядач Тестовий",
//                PasswordHash = PasswordHasher.Hash(UserPassword),
//                Role = UserRole.User,
//                CreatedAt = DateTime.UtcNow
//            });
//
//            DateTime tomorrow = DateTime.Today.AddDays(1);
//            await sessions.CreateAsync(new MovieSession { Id = ScheduledSessionId, MovieTitle = "Тіні забутих предків", BasePrice = 160, HallNumber = 1, DurationMinutes = 97, MinAge = 12, Status = MovieSessionStatus.Scheduled, StartTime = tomorrow.AddHours(10) });
//            await sessions.CreateAsync(new MovieSession { Id = SecondScheduledSessionId, MovieTitle = "Дюна: Частина друга", BasePrice = 220, HallNumber = 2, DurationMinutes = 166, MinAge = 16, Status = MovieSessionStatus.Scheduled, StartTime = tomorrow.AddHours(19) });
//            await sessions.CreateAsync(new MovieSession { Id = CompletedSessionId, MovieTitle = "Інтерстеллар", BasePrice = 180, HallNumber = 1, DurationMinutes = 169, MinAge = 12, Status = MovieSessionStatus.Completed, StartTime = DateTime.Today.AddDays(-3).AddHours(15) });
//            await sessions.CreateAsync(new MovieSession { Id = CancelledSessionId, MovieTitle = "Оппенгеймер", BasePrice = 200, HallNumber = 3, DurationMinutes = 180, MinAge = 16, Status = MovieSessionStatus.Cancelled, StartTime = tomorrow.AddHours(21) });
//
//            for (int i = 0; i < SeatIds.Length; i++)
//            {
//                await seats.CreateAsync(new CinemaHallSeat
//                {
//                    Id = SeatIds[i],
//                    MovieSessionId = ScheduledSessionId,
//                    RowNumber = i < 3 ? 1 : 2,
//                    SeatNumber = i < 3 ? i + 1 : 1,
//                    Category = i < 3 ? SeatCategory.Standard : SeatCategory.VIP,
//                    Status = SeatStatus.Free
//                });
//            }
//        }
//
//        public IMongoCollection<T> Collection<T>(string name)
//        {
//            return Database.GetCollection<T>(name);
//        }
//    }
//}

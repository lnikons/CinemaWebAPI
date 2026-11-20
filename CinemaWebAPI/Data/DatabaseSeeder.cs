using CinemaWebAPI.Models;
using CinemaWebAPI.Repositories;
using CinemaWebAPI.Services;
using CinemaWebAPI.Settings;
using Microsoft.Extensions.Options;

namespace CinemaWebAPI.Data
{
    public class DatabaseSeeder
    {
        private readonly IMovieSessionRepository _sessions;
        private readonly ICinemaHallSeatRepository _seats;
        private readonly IUserRepository _users;
        private readonly AdminSeedSettings _admin;

        public DatabaseSeeder(IMovieSessionRepository sessions, ICinemaHallSeatRepository seats,
            IUserRepository users, IOptions<AdminSeedSettings> adminOptions)
        {
            _sessions = sessions;
            _seats = seats;
            _users = users;
            _admin = adminOptions.Value;
        }

        public async Task SeedAsync()
        {
            await SeedAdminAsync();
            await SeedSessionsAsync();
        }

        private async Task SeedAdminAsync()
        {
            if (string.IsNullOrWhiteSpace(_admin.Email) || await _users.ExistsWithRoleAsync(UserRole.Admin))
                return;

            await _users.CreateAsync(new User
            {
                Email = _admin.Email.Trim().ToLowerInvariant(),
                FullName = _admin.FullName,
                PasswordHash = PasswordHasher.Hash(_admin.Password),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            });
        }

        private async Task SeedSessionsAsync()
        {
            if ((await _sessions.GetAllAsync()).Count > 0)
                return;

            DateTime tomorrow = DateTime.Today.AddDays(1);
            MovieSession[] sessions =
            {
                new MovieSession { MovieTitle = "Тіні забутих предків", BasePrice = 160, HallNumber = 1, DurationMinutes = 97, Status = MovieSessionStatus.Scheduled, StartTime = tomorrow.AddHours(10) },
                new MovieSession { MovieTitle = "Дюна: Частина друга", BasePrice = 220, HallNumber = 2, DurationMinutes = 166, Status = MovieSessionStatus.Scheduled, StartTime = tomorrow.AddHours(19).AddMinutes(30) },
                new MovieSession { MovieTitle = "Інтерстеллар", BasePrice = 180, HallNumber = 1, DurationMinutes = 169, Status = MovieSessionStatus.Scheduled, StartTime = tomorrow.AddHours(15) }
            };

            foreach (MovieSession session in sessions)
            {
                MovieSession created = await _sessions.CreateAsync(session);

                for (int number = 1; number <= 4; number++)
                    await _seats.CreateAsync(new CinemaHallSeat { MovieSessionId = created.Id, RowNumber = 1, SeatNumber = number, Category = SeatCategory.Standard, Status = SeatStatus.Free });
                for (int number = 1; number <= 2; number++)
                    await _seats.CreateAsync(new CinemaHallSeat { MovieSessionId = created.Id, RowNumber = 2, SeatNumber = number, Category = SeatCategory.VIP, Status = SeatStatus.Free });
            }
        }
    }
}

//using System.Net;
//using System.Net.Http.Json;
//using CinemaWebAPI.Documents;
//using CinemaWebAPI.Dtos;
//using CinemaWebAPI.IntegrationTests.Infrastructure;
//using CinemaWebAPI.Models;
//using CinemaWebAPI.Repositories;
//using Microsoft.AspNetCore.Mvc;
//using MongoDB.Driver;
//
//namespace CinemaWebAPI.IntegrationTests.Controllers
//{
//    public class MovieSessionControllerTests : IntegrationTestBase
//    {
//        private const string BaseUrl = "/api/movieSessions";
//
//        public MovieSessionControllerTests(ApiFactory factory) : base(factory) { }
//
//        private IMongoCollection<MovieSessionDocument> Sessions => Factory.Collection<MovieSessionDocument>(MovieSessionRepository.CollectionName);
//
//        private async Task<MovieSessionDocument?> FindAsync(string id)
//        {
//            return await Sessions.Find(Builders<MovieSessionDocument>.Filter.Eq(d => d.Id, id)).FirstOrDefaultAsync();
//        }
//
//        private static CreateMovieSessionDto ValidCreateDto()
//        {
//            return new CreateMovieSessionDto
//            {
//                MinAge = 12,
//                MovieTitle = "Джеймс Бонд: Казино Рояль",
//                BasePrice = 150,
//                HallNumber = 4,
//                DurationMinutes = 144,
//                StartTime = DateTime.Now.AddDays(2)
//            };
//        }
//
//        private static UpdateMovieSessionDto ValidUpdateDto()
//        {
//            return new UpdateMovieSessionDto
//            {
//                MinAge = 16,
//                MovieTitle = "Оновлена назва",
//                BasePrice = 250,
//                HallNumber = 5,
//                DurationMinutes = 100,
//                Status = MovieSessionStatus.Ongoing,
//                StartTime = DateTime.Now.AddDays(3)
//            };
//        }
//
//        [Fact]
//        public async Task GetAll_Anonymous_Returns200WithSeededSessions()
//        {
//            HttpClient client = CreateAnonymousClient();
//
//            HttpResponseMessage response = await client.GetAsync(BaseUrl);
//
//            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
//            List<MovieSessionResponseDto> sessions = await ReadAsync<List<MovieSessionResponseDto>>(response);
//            Assert.Equal(4, sessions.Count);
//            Assert.Contains(sessions, s => s.Id == ApiFactory.ScheduledSessionId && s.MovieTitle == "Тіні забутих предків");
//        }
//
//        [Fact]
//        public async Task GetAll_UnknownSortValue_Returns400()
//        {
//            HttpClient client = CreateAnonymousClient();
//
//            HttpResponseMessage response = await client.GetAsync($"{BaseUrl}?sort=title");
//
//            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//            ProblemDetails problem = await ReadAsync<ProblemDetails>(response);
//            Assert.Contains("title", problem.Detail);
//        }
//
//        [Fact]
//        public async Task GetById_ExistingSession_Returns200WithSessionData()
//        {
//            HttpClient client = CreateAnonymousClient();
//
//            HttpResponseMessage response = await client.GetAsync($"{BaseUrl}/{ApiFactory.ScheduledSessionId}");
//
//            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//            MovieSessionResponseDto session = await ReadAsync<MovieSessionResponseDto>(response);
//            Assert.Equal(ApiFactory.ScheduledSessionId, session.Id);
//            Assert.Equal("Тіні забутих предків", session.MovieTitle);
//            Assert.Equal(160, session.BasePrice);
//            Assert.Equal(MovieSessionStatus.Scheduled, session.Status);
//        }
//
//        [Fact]
//        public async Task GetById_NotExistingId_Returns404()
//        {
//            HttpClient client = CreateAnonymousClient();
//
//            HttpResponseMessage response = await client.GetAsync($"{BaseUrl}/{ApiFactory.MissingId}");
//
//            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
//            ProblemDetails problem = await ReadAsync<ProblemDetails>(response);
//            Assert.Equal(404, problem.Status);
//            Assert.Contains(ApiFactory.MissingId, problem.Detail);
//        }
//
//        [Fact]
//        public async Task Create_ValidRequestAsAdmin_Returns201AndSavesDocument()
//        {
//            HttpClient client = await CreateAdminClientAsync();
//            CreateMovieSessionDto dto = ValidCreateDto();
//
//            HttpResponseMessage response = await client.PostAsJsonAsync(BaseUrl, dto, Json);
//
//            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
//            MovieSessionResponseDto created = await ReadAsync<MovieSessionResponseDto>(response);
//            Assert.NotEmpty(created.Id);
//            Assert.EndsWith($"{BaseUrl}/{created.Id}", response.Headers.Location!.AbsolutePath);
//            Assert.Equal(MovieSessionStatus.Scheduled, created.Status);
//
//            MovieSessionDocument? saved = await FindAsync(created.Id);
//            Assert.NotNull(saved);
//            Assert.Equal(dto.MovieTitle, saved!.MovieTitle);
//            Assert.Equal(150, saved.BasePrice);
//            Assert.Equal(MovieSessionStatus.Scheduled, saved.Status);
//            Assert.Equal(5, await Sessions.CountDocumentsAsync(FilterDefinition<MovieSessionDocument>.Empty));
//        }
//
//        [Fact]
//        public async Task Create_InvalidRequest_Returns400WithValidationErrorsAndSavesNothing()
//        {
//            HttpClient client = await CreateAdminClientAsync();
//            CreateMovieSessionDto dto = new CreateMovieSessionDto
//            {
//                MinAge = 40,
//                MovieTitle = "A",
//                BasePrice = 10,
//                HallNumber = 99,
//                DurationMinutes = 5,
//                StartTime = DateTime.Now.AddDays(-1)
//            };
//
//            HttpResponseMessage response = await client.PostAsJsonAsync(BaseUrl, dto, Json);
//
//            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//            ValidationProblemDetails problem = await ReadAsync<ValidationProblemDetails>(response);
//            string[] fields = problem.Errors.Keys.Select(k => k.ToLowerInvariant()).ToArray();
//            Assert.Contains("movietitle", fields);
//            Assert.Contains("baseprice", fields);
//            Assert.Contains("hallnumber", fields);
//            Assert.Contains("durationminutes", fields);
//            Assert.Contains("starttime", fields);
//            Assert.Contains("minage", fields);
//            Assert.Equal(4, await Sessions.CountDocumentsAsync(FilterDefinition<MovieSessionDocument>.Empty));
//        }
//
//        [Fact]
//        public async Task Update_ExistingSession_Returns200AndReplacesDocument()
//        {
//            HttpClient client = await CreateAdminClientAsync();
//
//            HttpResponseMessage response = await client.PutAsJsonAsync($"{BaseUrl}/{ApiFactory.ScheduledSessionId}", ValidUpdateDto(), Json);
//
//            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//            MovieSessionResponseDto updated = await ReadAsync<MovieSessionResponseDto>(response);
//            Assert.Equal("Оновлена назва", updated.MovieTitle);
//            MovieSessionDocument? saved = await FindAsync(ApiFactory.ScheduledSessionId);
//            Assert.NotNull(saved);
//            Assert.Equal("Оновлена назва", saved!.MovieTitle);
//            Assert.Equal(250, saved.BasePrice);
//            Assert.Equal(5, saved.HallNumber);
//            Assert.Equal(MovieSessionStatus.Ongoing, saved.Status);
//        }
//
//        [Fact]
//        public async Task Update_NotExistingId_Returns404()
//        {
//            HttpClient client = await CreateAdminClientAsync();
//
//            HttpResponseMessage response = await client.PutAsJsonAsync($"{BaseUrl}/{ApiFactory.MissingId}", ValidUpdateDto(), Json);
//
//            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
//        }
//
//        [Fact]
//        public async Task Update_InvalidRequest_Returns400AndKeepsDocumentUnchanged()
//        {
//            HttpClient client = await CreateAdminClientAsync();
//            UpdateMovieSessionDto dto = ValidUpdateDto();
//            dto.BasePrice = 5;
//            dto.MovieTitle = "";
//
//            HttpResponseMessage response = await client.PutAsJsonAsync($"{BaseUrl}/{ApiFactory.ScheduledSessionId}", dto, Json);
//
//            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//            MovieSessionDocument? saved = await FindAsync(ApiFactory.ScheduledSessionId);
//            Assert.Equal("Тіні забутих предків", saved!.MovieTitle);
//            Assert.Equal(160, saved.BasePrice);
//        }
//
//        [Fact]
//        public async Task Patch_OnlyTitle_Returns200AndChangesOnlyTitle()
//        {
//            HttpClient client = await CreateAdminClientAsync();
//            StringContent body = new StringContent("""{"movieTitle":"Нова назва сеансу"}""", System.Text.Encoding.UTF8, "application/json");
//
//            HttpResponseMessage response = await client.PatchAsync($"{BaseUrl}/{ApiFactory.SecondScheduledSessionId}", body);
//
//            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
//            MovieSessionDocument? saved = await FindAsync(ApiFactory.SecondScheduledSessionId);
//            Assert.Equal("Нова назва сеансу", saved!.MovieTitle);
//            Assert.Equal(220, saved.BasePrice);
//            Assert.Equal(2, saved.HallNumber);
//            Assert.Equal(166, saved.DurationMinutes);
//            Assert.Equal(MovieSessionStatus.Scheduled, saved.Status);
//        }
//
//        [Fact]
//        public async Task Patch_PriceOutOfRange_Returns400AndKeepsDocumentUnchanged()
//        {
//            HttpClient client = await CreateAdminClientAsync();
//            StringContent body = new StringContent("""{"basePrice":5000}""", System.Text.Encoding.UTF8, "application/json");
//
//            HttpResponseMessage response = await client.PatchAsync($"{BaseUrl}/{ApiFactory.ScheduledSessionId}", body);
//
//            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
//            MovieSessionDocument? saved = await FindAsync(ApiFactory.ScheduledSessionId);
//            Assert.Equal(160, saved!.BasePrice);
//        }
//
//        [Fact]
//        public async Task Patch_NotExistingId_Returns404()
//        {
//            HttpClient client = await CreateAdminClientAsync();
//            StringContent body = new StringContent("""{"movieTitle":"Назва"}""", System.Text.Encoding.UTF8, "application/json");
//
//            HttpResponseMessage response = await client.PatchAsync($"{BaseUrl}/{ApiFactory.MissingId}", body);
//
//            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
//        }
//
//        [Theory]
//        [InlineData(ApiFactory.CompletedSessionId)]
//        [InlineData(ApiFactory.CancelledSessionId)]
//        public async Task Delete_CompletedOrCancelledSession_Returns204AndRemovesDocument(string id)
//        {
//            HttpClient client = await CreateAdminClientAsync();
//
//            HttpResponseMessage response = await client.DeleteAsync($"{BaseUrl}/{id}");
//
//            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
//            Assert.Null(await FindAsync(id));
//            Assert.Equal(3, await Sessions.CountDocumentsAsync(FilterDefinition<MovieSessionDocument>.Empty));
//        }
//
//        [Fact]
//        public async Task Delete_NotExistingId_Returns404()
//        {
//            HttpClient client = await CreateAdminClientAsync();
//
//            HttpResponseMessage response = await client.DeleteAsync($"{BaseUrl}/{ApiFactory.MissingId}");
//
//            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
//        }
//
//        [Fact]
//        public async Task Database_IsResetBeforeEachTest_StartsWithSeededSessions()
//        {
//            long count = await Sessions.CountDocumentsAsync(FilterDefinition<MovieSessionDocument>.Empty);
//
//            Assert.Equal(4, count);
//            Assert.Equal("Тіні забутих предків", (await FindAsync(ApiFactory.ScheduledSessionId))!.MovieTitle);
//        }
//    }
//}

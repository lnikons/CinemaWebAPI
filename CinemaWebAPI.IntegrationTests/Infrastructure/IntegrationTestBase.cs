//using System.Net.Http.Headers;
//using System.Net.Http.Json;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using CinemaWebAPI.Dtos;
//
//namespace CinemaWebAPI.IntegrationTests.Infrastructure
//{
//    [CollectionDefinition(Name)]
//    public class IntegrationCollection : ICollectionFixture<ApiFactory>
//    {
//        public const string Name = "Integration";
//    }
//
//    [Collection(IntegrationCollection.Name)]
//    public abstract class IntegrationTestBase : IAsyncLifetime
//    {
//        protected static readonly JsonSerializerOptions Json = new JsonSerializerOptions(JsonSerializerDefaults.Web)
//        {
//            Converters = { new JsonStringEnumConverter() }
//        };
//
//        protected readonly ApiFactory Factory;
//
//        protected IntegrationTestBase(ApiFactory factory)
//        {
//            Factory = factory;
//        }
//
//        public Task InitializeAsync() => Factory.ResetDatabaseAsync();
//
//        public Task DisposeAsync() => Task.CompletedTask;
//
//        protected HttpClient CreateAnonymousClient()
//        {
//            return Factory.CreateClient();
//        }
//
//        protected Task<HttpClient> CreateAdminClientAsync()
//        {
//            return CreateAuthorizedClientAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);
//        }
//
//        protected Task<HttpClient> CreateUserClientAsync()
//        {
//            return CreateAuthorizedClientAsync(ApiFactory.UserEmail, ApiFactory.UserPassword);
//        }
//
//        private async Task<HttpClient> CreateAuthorizedClientAsync(string email, string password)
//        {
//            HttpClient client = Factory.CreateClient();
//            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = email, Password = password });
//            response.EnsureSuccessStatusCode();
//
//            AuthResponseDto auth = (await response.Content.ReadFromJsonAsync<AuthResponseDto>(Json))!;
//            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
//            return client;
//        }
//
//        protected static async Task<T> ReadAsync<T>(HttpResponseMessage response)
//        {
//            return (await response.Content.ReadFromJsonAsync<T>(Json))!;
//        }
//    }
//}

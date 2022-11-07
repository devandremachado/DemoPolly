namespace DemoPolly.ExternalServices
{
    public interface IAnyApiService
    {
        Task<object> GetSomethingWithException();
        Task<object> GetSomethingWithSuccess();
    }

    public class AnyApiService : IAnyApiService
    {
        private readonly HttpClient _client;

        public AnyApiService(HttpClient client)
        {
            _client = client;
        }

        public async Task<object> GetSomethingWithException()
        {
            var response = await _client.GetAsync("http://httpstat.us/408");

            return response;
        }

        public async Task<object> GetSomethingWithSuccess()
        {
            var response = await _client.GetAsync("http://httpstat.us/200");

            return response;
        }
    }
}

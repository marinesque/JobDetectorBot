namespace VacancyService.HeadHunterApiClient
{
    public class HeadHunterServiceClientFactory : IServiceClientFactory
    {
        private readonly string _baseUrl;
        private readonly string _apiKey;
		private readonly IHttpClientFactory _httpClientFactory;

		public HeadHunterServiceClientFactory(string baseUrl, string apiKey, IHttpClientFactory httpClientFactory)
        {
            _baseUrl = baseUrl;
            _apiKey = apiKey;
			_httpClientFactory = httpClientFactory;
		}

        public IServiceClient CreateClient()
        {
            return new ServiceClient(_baseUrl, _apiKey, _httpClientFactory);
        }
    }
}
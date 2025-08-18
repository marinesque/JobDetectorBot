using Bot.Domain.DataAccess.Dto;
using Bot.Domain.Request.VacancySearch;
using Bot.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Net.Http.Json;
using System.Text.Json;

public class VacancySearchService : IVacancySearchService
{
    private readonly HttpClient _httpClient;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<VacancySearchService> _logger;
    private readonly string _apiBaseUrl;

    public VacancySearchService(HttpClient httpClient, IConnectionMultiplexer redis,
        ILogger<VacancySearchService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _redis = redis;
        _logger = logger;
        _apiBaseUrl = configuration["VacancySearchService:BaseUrl"];
    }

    public async Task<bool> SearchAndCacheVacancies(long userId, UserCriteriaRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/search", request);
            response.EnsureSuccessStatusCode();

            var vacancies = await response.Content.ReadFromJsonAsync<List<VacancyDto>>();
            var db = _redis.GetDatabase();

            // Сохраняем с нумерацией
            for (int i = 0; i < vacancies.Count; i++)
            {
                await db.HashSetAsync($"vacancies:{userId}", i, JsonSerializer.Serialize(vacancies[i]));
            }

            return vacancies.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search error");
            return false;
        }
    }

    public async Task<List<VacancyDto>> GetVacanciesPage(long userId, int page, int pageSize = 5)
    {
        var db = _redis.GetDatabase();
        var start = (page - 1) * pageSize;
        var end = start + pageSize - 1;

        var results = new List<VacancyDto>();
        for (int i = start; i <= end; i++)
        {
            var json = await db.HashGetAsync($"vacancies:{userId}", i);
            if (json.HasValue)
            {
                results.Add(JsonSerializer.Deserialize<VacancyDto>(json));
            }
        }

        return results;
    }

    public async Task ClearCache(long userId)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync($"vacancies:{userId}");
    }
}
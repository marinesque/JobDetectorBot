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
        _httpClient.BaseAddress = new Uri(_apiBaseUrl);
    }

    public async Task<bool> SearchAndCacheVacancies(long userId, UserCriteriaRequest request)
    {
        try
        {
            _logger.LogInformation($"Критерии поиска для пользователя {userId}:");
            foreach (var criteria in request.UserCriteria)
            {
                _logger.LogInformation($"- {criteria.Name}: {criteria.Id} (Custom: {criteria.IsCustom}, Mapped: {criteria.IsMapped})");
            }

            var searchCriteria = request.UserCriteria.FirstOrDefault(x => x.Name == "post");
            var search = searchCriteria?.Id ?? "";

            _logger.LogInformation($"Извлеченная должность для поиска: '{search}'");

            if (string.IsNullOrEmpty(search))
            {
                _logger.LogWarning("Должность не указана для поиска");
                return false;
            }

            var experience = request.UserCriteria.FirstOrDefault(x => x.Name == "experience")?.Id;
            var employment = request.UserCriteria.FirstOrDefault(x => x.Name == "employment")?.Id;
            var schedule = request.UserCriteria.FirstOrDefault(x => x.Name == "schedule")?.Id;
            var salaryRangeFrequency = request.UserCriteria.FirstOrDefault(x => x.Name == "salaryRangeFrequency")?.Id;

            _logger.LogInformation($"Дополнительные параметры: experience={experience}, employment={employment}, schedule={schedule}, salaryRangeFrequency={salaryRangeFrequency}");

            int? salary = null;
            var salaryCriteria = request.UserCriteria.FirstOrDefault(x => x.Name == "salary");
            if (salaryCriteria != null && int.TryParse(salaryCriteria.Id, out int salaryValue))
            {
                salary = salaryValue;
            }

            //var url = $"api/vacancies/{Uri.EscapeDataString(search)}";
            var encodedSearch = Uri.EscapeDataString(search);
            var url = $"{_apiBaseUrl}/Vacancy/{encodedSearch}";

            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(experience))
                queryParams.Add($"experience={Uri.EscapeDataString(experience)}");

            if (!string.IsNullOrEmpty(employment))
                queryParams.Add($"employment={Uri.EscapeDataString(employment)}");

            if (!string.IsNullOrEmpty(schedule))
                queryParams.Add($"schedule={Uri.EscapeDataString(schedule)}");

            if (!string.IsNullOrEmpty(salaryRangeFrequency))
                queryParams.Add($"salaryRangeFrequency={Uri.EscapeDataString(salaryRangeFrequency)}");

            if (salary.HasValue)
                queryParams.Add($"salary={salary.Value}");

            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            _logger.LogInformation($"Запрос к API: {url}");

            // API
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Ошибка API: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return false;
            }

            var vacancies = await response.Content.ReadFromJsonAsync<List<Bot.Domain.DataAccess.Model.Vacancy>>();

            if (vacancies == null || !vacancies.Any())
            {
                _logger.LogInformation("Вакансии не найдены");
                return false;
            }

            // Redis
            var db = _redis.GetDatabase();
            var redisKey = $"vacancies:{userId}";

            await db.KeyDeleteAsync(redisKey);

            // Новые
            for (int i = 0; i < vacancies.Count; i++)
            {
                await db.HashSetAsync(redisKey, i, JsonSerializer.Serialize(vacancies[i]));
            }

            _logger.LogInformation($"Найдено и закэшировано {vacancies.Count} вакансий для пользователя {userId}");
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"Ошибка подключения к сервису поиска вакансий: {_apiBaseUrl}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске вакансий");
            return false;
        }
    }


    public async Task<List<VacancyDto>> GetVacanciesPage(long userId, int page, int pageSize = 5)
    {
        try
        {
            var db = _redis.GetDatabase();
            var redisKey = $"vacancies:{userId}";
            var start = (page - 1) * pageSize;
            var end = start + pageSize - 1;

            var results = new List<VacancyDto>();

            for (int i = start; i <= end; i++)
            {
                var json = await db.HashGetAsync(redisKey, i);
                if (json.HasValue)
                {
                    var vacancy = JsonSerializer.Deserialize<Bot.Domain.DataAccess.Model.Vacancy>(json);
                    if (vacancy != null)
                    {
                        results.Add(new VacancyDto
                        {
                            Title = vacancy.Name,
                            CompanyName = vacancy.Employer,
                            Location = vacancy.Area,
                            Schedule = vacancy.Schedule,
                            WorkFormat = vacancy.WorkFormat?.FirstOrDefault() ?? "Не указан",
                            Url = vacancy.Link,
                            PublishedDate = vacancy.PublishedVacancyDate
                        });
                    }
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении страницы вакансий.");
            return new List<VacancyDto>();
        }
    }

    public async Task ClearCache(long userId)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync($"vacancies:{userId}");
            _logger.LogInformation($"Кэш вакансий очищен для пользователя {userId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при очистке кэша вакансий.");
        }
    }
}
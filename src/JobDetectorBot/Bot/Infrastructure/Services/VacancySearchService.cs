using Bot.Domain.DataAccess.Model;
using Bot.Domain.DataAccess.Repositories;
using Bot.Domain.Request.VacancySearch;
using Bot.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace Bot.Infrastructure.Services
{
    public class VacancySearchService : IVacancySearchService
    {
        private readonly HttpClient _httpClient;
        private readonly UserRepository _userRepository;
        private readonly ILogger<VacancySearchService> _logger;
        private readonly string _apiBaseUrl;

        public VacancySearchService(
            HttpClient httpClient,
            UserRepository userRepository,
            ILogger<VacancySearchService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _userRepository = userRepository;
            _logger = logger;
            _apiBaseUrl = configuration["VacancySearchService:BaseUrl"] ?? throw new ArgumentNullException("VacancySearchService:BaseUrl не настроен");
        }

        public async Task SendUserCriteriaToSearchService(long userId)
        {
            try
            {
                var user = await _userRepository.GetUserAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь {userId} не найден!");
                    return;
                }

                var criteriaData = new UserCriteriaRequest
                {
                    UserId = userId,
                    RequestDate = DateTime.UtcNow,
                    UserCriteria = new List<UserCriteriaItem>()
                };

                foreach (var userCriteria in user.UserCriteriaStepValues)
                {
                    var isCustom = !string.IsNullOrEmpty(userCriteria.CustomValue);

                    criteriaData.UserCriteria.Add(new UserCriteriaItem
                    {
                        Name = userCriteria.CriteriaStep.Name,
                        Id = isCustom ? userCriteria.CustomValue : userCriteria.CriteriaStepValue?.Value,
                        IsCustom = isCustom,
                        IsMapped = userCriteria.CriteriaStep.IsMapped,
                        MainDictionary = userCriteria.CriteriaStep.MainDictionary
                    });
                }

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_apiBaseUrl}/api/user-criteria",
                    criteriaData,
                    jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Ошибка при отправке критериев! Статус: {response.StatusCode}, Ошибка: {errorContent}");
                }
                else
                {
                    _logger.LogInformation($"Критерии пользователя {userId} успешно отправлены");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при отправке критериев пользователя {userId}");
                throw;
            }
        }

        public async Task<List<Vacancy>> SearchVacancies(UserCriteriaRequest request)
        {
            try
            {
                // Получаем пользователя с его критериями
                var user = await _userRepository.GetUserAsync(request.UserId);
                if (user == null || user.UserCriteriaStepValues == null || !user.UserCriteriaStepValues.Any())
                {
                    _logger.LogWarning($"Пользователь {request.UserId} не найден или не имеет критериев");
                    return new List<Vacancy>();
                }

                // Извлекаем нужные параметры из критериев пользователя
                string search = GetCriteriaValue(user, "Post");
                string experience = GetCriteriaValue(user, "Experience");
                string employment = GetCriteriaValue(user, "Employment");
                string schedule = GetCriteriaValue(user, "Schedule");
                int? salary = GetSalaryValue(user);

                // Формируем URL запроса с параметрами
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (!string.IsNullOrEmpty(experience)) queryParams.Add($"experience={Uri.EscapeDataString(experience)}");
                if (!string.IsNullOrEmpty(employment)) queryParams.Add($"employment={Uri.EscapeDataString(employment)}");
                if (!string.IsNullOrEmpty(schedule)) queryParams.Add($"schedule={Uri.EscapeDataString(schedule)}");
                if (salary.HasValue) queryParams.Add($"salary={salary.Value}");

                var queryString = queryParams.Any() ? $"?{string.Join("&", queryParams)}" : "";
                var url = $"{_apiBaseUrl}/vacancy{queryString}";

                // Отправляем GET запрос
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Ошибка при поиске вакансий! Статус: {response.StatusCode}, Ошибка: {errorContent}");
                    return new List<Vacancy>();
                }

                return await response.Content.ReadFromJsonAsync<List<Vacancy>>() ?? new List<Vacancy>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при поиске вакансий для пользователя {request.UserId}");
                throw;
            }
        }

        private string GetCriteriaValue(User user, string criteriaName)
        {
            var criteria = user.UserCriteriaStepValues
                .FirstOrDefault(uc => uc.CriteriaStep?.Name == criteriaName);

            if (criteria == null) return null;

            // Предпочтение отдается кастомному значению
            return !string.IsNullOrEmpty(criteria.CustomValue)
                ? criteria.CustomValue
                : criteria.CriteriaStepValue?.Value;
        }

        private int? GetSalaryValue(User user)
        {
            var salaryCriteria = user.UserCriteriaStepValues
                .FirstOrDefault(uc => uc.CriteriaStep?.Name == "Salary");

            if (salaryCriteria == null) return null;

            try
            {
                // Пытаемся получить значение из CustomValue или Value
                var salaryValue = !string.IsNullOrEmpty(salaryCriteria.CustomValue)
                    ? salaryCriteria.CustomValue
                    : salaryCriteria.CriteriaStepValue?.Value;

                if (string.IsNullOrEmpty(salaryValue)) return null;

                // Пытаемся преобразовать в число
                if (decimal.TryParse(salaryValue, out var decimalValue))
                {
                    return (int)Math.Round(decimalValue);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
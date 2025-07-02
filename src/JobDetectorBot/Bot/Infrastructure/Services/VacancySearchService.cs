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

        //TODO: Настроить апи
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

        /// <summary>
        /// Отправка запроса в сервис работы с вакансиями
        /// Формат запроса: {
        ///   "UserId": Users.Id,
        ///   "RequestDate": DateTime.UtcNow
        ///   "UserCriteria": [{
        ///     "Name": CriteriaStep.Name,
        ///     "Id": если кастомное значение userCriteria.CustomValue, если нет userCriteria.CriteriaStepValue?.Value,
        ///     "IsCustom": !string.IsNullOrEmpty(UserCriteriaStepValue.CustomValue),
        ///     "IsMapped": CriteriaStep.IsMapped,
        ///     "MainDictionary": CriteriaStep.MainDictionary - заполняется, если критерий входит в справочник
        ///   }]
        /// }
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
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
                // TODO: Дописать точный адрес
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}/api/vacancies?userId={request.UserId}");

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
    }
}
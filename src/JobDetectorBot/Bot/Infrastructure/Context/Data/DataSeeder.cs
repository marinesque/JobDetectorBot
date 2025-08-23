using Bot.Domain.DataAccess.Model;
using Bot.Domain.DataAccess.Repositories;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

public class DataSeeder
{
    private readonly CriteriaStepRepository _criteriaStepRepository;
    private readonly ILogger<DataSeeder> _logger;
    private readonly string _filePath;
    private readonly object _seedLock = new object();
    private volatile bool _isSeeded = false; // volatile видимость флага между потоками
    private readonly ManualResetEvent _seedingCompleted = new ManualResetEvent(false); // Другие компоненты будут ждать завершения актуализации
    private bool _disposed = false;

    public DataSeeder(
            CriteriaStepRepository criteriaStepRepository,
            ILogger<DataSeeder> logger)
    {
        _criteriaStepRepository = criteriaStepRepository;
        _logger = logger;

        _filePath = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Context", "Data", "CriteriaStepData.json");
    }

    public async Task SeedDataAsync()
    {
        if (_isSeeded)
        {
            _logger.LogInformation("Данные уже были актуализированы ранее.");
            return;
        }

        lock (_seedLock)
        {
            if (_isSeeded)
            {
                _logger.LogInformation("Данные уже были актуализированы.");
                return;
            }

            _logger.LogInformation("Начало процесса актуализации данных.");
        }

        try
        {
            await SeedDataInternalAsync();

            lock (_seedLock)
            {
                _isSeeded = true;
                _seedingCompleted.Set(); //Сигнализируем, что процесс завершен
            }

            _logger.LogInformation("Актуализация данных успешно завершена.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении актуализации данных.");
            lock (_seedLock)
            {
                _seedingCompleted.Reset();
            }
            throw;
        }
    }

    private async Task SeedDataInternalAsync()
    {
        if (!File.Exists(_filePath))
        {
            var errorMessage = $"Файл данных не найден: {_filePath}.";
            _logger.LogError(errorMessage);
            throw new FileNotFoundException(errorMessage, _filePath);
        }

        _logger.LogInformation("Чтение файла данных: {FilePath}.", _filePath);

        // Чтение файла с блокировкой
        string json;
        lock (_seedLock)
        {
            json = File.ReadAllText(_filePath, Encoding.UTF8);
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogError("Файл данных пуст: {FilePath}!!!", _filePath);
            return;
        }

        var seedData = JsonSerializer.Deserialize<CriteriaStepData>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

        if (seedData?.CriteriaSteps == null || !seedData.CriteriaSteps.Any())
        {
            _logger.LogWarning("Нет данных для seeding в файле: {FilePath}.", _filePath);
            return;
        }

        foreach (var step in seedData.CriteriaSteps)
        {
            try
            {
                await ProcessCriteriaStepAsync(step);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке критерия: {CriteriaName}!!!", step.Name);
            }
        }

        _logger.LogInformation("Обработка всех критериев завершена");
    }

    private async Task ProcessCriteriaStepAsync(CriteriaStep step)
    {
        _logger.LogDebug("Обработка критерия: {Name} - {Prompt}.", step.Name, step.Prompt);

        var existingStep = await _criteriaStepRepository.GetCriteriaStepByNameAsync(step.Name);

        if (existingStep != null)
        {
            await UpdateExistingCriteriaStepAsync(existingStep, step);
        }
        else
        {
            await AddNewCriteriaStepAsync(step);
        }
    }

    private async Task UpdateExistingCriteriaStepAsync(CriteriaStep existingStep, CriteriaStep newStep)
    {
        _logger.LogDebug("Обновление существующего критерия: {Name}.", existingStep.Name);

        existingStep.Prompt = newStep.Prompt;
        existingStep.IsCustom = newStep.IsCustom;
        existingStep.OrderBy = newStep.OrderBy;
        existingStep.Type = newStep.Type;
        existingStep.IsMapped = newStep.IsMapped;
        existingStep.MainDictionary = newStep.MainDictionary;

        if (newStep.CriteriaStepValues != null)
        {
            foreach (var newValue in newStep.CriteriaStepValues)
            {
                var existingValue = existingStep.CriteriaStepValues?
                    .FirstOrDefault(v => v.Value == newValue.Value);

                if (existingValue != null)
                {
                    existingValue.Prompt = newValue.Prompt;
                    existingValue.OrderBy = newValue.OrderBy;
                }
                else
                {
                    existingStep.CriteriaStepValues ??= new List<CriteriaStepValue>();
                    existingStep.CriteriaStepValues.Add(new CriteriaStepValue
                    {
                        CriteriaStepId = existingStep.Id,
                        Prompt = newValue.Prompt,
                        Value = newValue.Value,
                        OrderBy = newValue.OrderBy
                    });
                }
            }
        }

        await _criteriaStepRepository.SaveChangesAsync();
        _logger.LogInformation("Критерий обновлен: {Name}.", existingStep.Name);
    }

    private async Task AddNewCriteriaStepAsync(CriteriaStep step)
    {
        _logger.LogDebug("Добавление нового критерия: {Name}.", step.Name);

        var stepToAdd = new CriteriaStep
        {
            Name = step.Name,
            Prompt = step.Prompt,
            IsCustom = step.IsCustom,
            OrderBy = step.OrderBy,
            Type = step.Type,
            IsMapped = step.IsMapped,
            MainDictionary = step.MainDictionary,
            CriteriaStepValues = step.CriteriaStepValues?.Select(v => new CriteriaStepValue
            {
                Prompt = v.Prompt,
                Value = v.Value,
                OrderBy = v.OrderBy
            }).ToList() ?? new List<CriteriaStepValue>()
        };

        await _criteriaStepRepository.AddOrUpdateCriteriaStepAsync(stepToAdd);
        _logger.LogInformation("Новый критерий добавлен: {Name}.", step.Name);
    }

    /// <summary>
    /// Ожидает завершения процесса актуализации с указанным таймаутом
    /// </summary>
    public bool WaitForSeeding(TimeSpan timeout)
    {
        return _seedingCompleted.WaitOne(timeout);
    }

    /// <summary>
    /// Ожидает завершения процесса актуализации без таймаута
    /// </summary>
    public void WaitForSeeding()
    {
        _seedingCompleted.WaitOne();
    }

    /// <summary>
    /// Проверяет, завершен ли процесс актуализации без ожидания
    /// </summary>
    public bool IsSeedingCompleted()
    {
        return _seedingCompleted.WaitOne(0);
    }

    public bool IsSeeded => _isSeeded;

    /// <summary>
    /// Сбрасывает состояние seeding
    /// </summary>
    public void Reset()
    {
        lock (_seedLock)
        {
            _isSeeded = false;
            _seedingCompleted.Reset();
        }
    }

    /// <summary>
    /// Освобождает ресурсы ManualResetEvent
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _seedingCompleted?.Close();
            }
            _disposed = true;
        }
    }

    ~DataSeeder()
    {
        Dispose(false);
    }

    public class CriteriaStepData
    {
        public List<CriteriaStep> CriteriaSteps { get; set; } = new List<CriteriaStep>();
    }
}
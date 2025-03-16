using Bot.Domain.DataAccess.Model;
using Bot.Domain.DataAccess.Repositories;
using System.Text;
using System.Text.Json;

public class DataSeeder
{
    private readonly CriteriaStepRepository _criteriaStepRepository;

    public DataSeeder(CriteriaStepRepository criteriaStepRepository)
    {
        _criteriaStepRepository = criteriaStepRepository;
    }

    public async Task SeedDataAsync()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Context", "Data", "CriteriaStepData.json");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Файл {filePath} не найден.");
        }

        var json = File.ReadAllText(filePath, Encoding.UTF8);
        var seedData = JsonSerializer.Deserialize<CriteriaStepData>(json);

        foreach (var step in seedData.CriteriaSteps)
        {
            await _criteriaStepRepository.AddOrUpdateCriteriaStepAsync(step);
        }
    }

    public class CriteriaStepData
    {
        public List<CriteriaStep> CriteriaSteps { get; set; }
    }
}
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
            var existingStep = await _criteriaStepRepository.GetCriteriaStepByNameAsync(step.Name);
            if (existingStep != null)
            {
                existingStep.Prompt = step.Prompt;
                existingStep.IsCustom = step.IsCustom;
                existingStep.OrderBy = step.OrderBy;
                existingStep.Type = step.Type;

                foreach (var value in step.CriteriaStepValues)
                {
                    var existingValue = existingStep.CriteriaStepValues
                        .FirstOrDefault(csv => csv.CriteriaStepId == existingStep.Id && csv.Value == value.Value);

                    if (existingValue != null)
                    {
                        existingValue.Prompt = value.Prompt;
                        existingValue.OrderBy = value.OrderBy;
                    }
                    else
                    {
                        existingStep.CriteriaStepValues.Add(new CriteriaStepValue
                        {
                            CriteriaStepId = existingStep.Id,
                            Prompt = value.Prompt,
                            Value = value.Value,
                            OrderBy = value.OrderBy
                        });
                    }
                }
            }
            else
            {
                await _criteriaStepRepository.AddOrUpdateCriteriaStepAsync(step);
            }

            await _criteriaStepRepository.SaveChangesAsync();
        }
    }

    public class CriteriaStepData
    {
        public List<CriteriaStep> CriteriaSteps { get; set; }
    }
}
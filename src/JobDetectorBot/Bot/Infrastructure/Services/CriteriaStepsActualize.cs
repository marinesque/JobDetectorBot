using Bot.Domain.DataAccess.Model;
using Bot.Domain.DataAccess.Repositories;
using Bot.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

public class CriteriaStepsActualize : ICriteriaStepsActualize, IDisposable
{
    private readonly ILogger<CriteriaStepsActualize> _logger;
    private readonly CriteriaStepRepository _criteriaStepRepository;
    private Timer _timer;
    private List<CriteriaStep> _criteriaSteps;
    public List<CriteriaStep> GetCriteriaSteps() => _criteriaSteps;

    public CriteriaStepsActualize(
        ILogger<CriteriaStepsActualize> logger,
        CriteriaStepRepository criteriaStepRepository)
    {
        _logger = logger;
        _criteriaStepRepository = criteriaStepRepository;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CriteriaStepsForegroundService начал обновление.");
        _timer = new Timer(ActualizeAsync, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    private async void ActualizeAsync(object state)
    {
        _logger.LogInformation("Обновление критериев в процессе...");
        _criteriaSteps = await _criteriaStepRepository.GetAllCriteriaStepsAsync();
        _logger.LogInformation("Обновление критериев завершено.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("CriteriaStepsForegroundService закончил обновление.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
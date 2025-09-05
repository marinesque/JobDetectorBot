namespace VacancyService.HeadHunterApiClient
{
    public interface IServiceClientFactory
    {
        IServiceClient CreateClient();
    }
}
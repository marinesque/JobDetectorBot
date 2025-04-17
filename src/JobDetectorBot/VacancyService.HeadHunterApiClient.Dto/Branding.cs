using Newtonsoft.Json; 
namespace VacancyService.HeadHunterApiClient.Dto{ 

    public class Branding
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type;

        [JsonProperty("tariff", NullValueHandling = NullValueHandling.Ignore)]
        public object Tariff;
    }

}
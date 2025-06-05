using Newtonsoft.Json; 
namespace VacancyService.HeadHunterApiClient.Dto{ 

    public class ProfessionalRole
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name;
    }

}
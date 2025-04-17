using Newtonsoft.Json; 
namespace VacancyService.HeadHunterApiClient.Dto{ 

    public class Snippet
    {
        [JsonProperty("requirement", NullValueHandling = NullValueHandling.Ignore)]
        public string Requirement;

        [JsonProperty("responsibility", NullValueHandling = NullValueHandling.Ignore)]
        public string Responsibility;
    }

}
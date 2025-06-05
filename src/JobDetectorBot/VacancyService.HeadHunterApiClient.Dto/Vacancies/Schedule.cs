using Newtonsoft.Json; 
namespace VacancyService.HeadHunterApiClient.Dto{ 

    public class Schedule
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name;
    }

}
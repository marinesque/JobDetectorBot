using Newtonsoft.Json; 
namespace VacancyService.HeadHunterApiClient.Dto{ 

    public class Area
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name;

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url;
    }

}
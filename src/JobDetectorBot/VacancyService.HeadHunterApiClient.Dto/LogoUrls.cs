using Newtonsoft.Json; 
namespace VacancyService.HeadHunterApiClient.Dto{ 

    public class LogoUrls
    {
        [JsonProperty("original", NullValueHandling = NullValueHandling.Ignore)]
        public string Original;

        [JsonProperty("90", NullValueHandling = NullValueHandling.Ignore)]
        public string _90;

        [JsonProperty("240", NullValueHandling = NullValueHandling.Ignore)]
        public string _240;
    }

}
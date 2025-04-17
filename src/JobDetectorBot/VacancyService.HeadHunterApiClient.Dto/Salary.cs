using Newtonsoft.Json; 
namespace VacancyService.HeadHunterApiClient.Dto{ 

    public class Salary
    {
        [JsonProperty("from", NullValueHandling = NullValueHandling.Ignore)]
        public int? From;

        [JsonProperty("to", NullValueHandling = NullValueHandling.Ignore)]
        public int? To;

        [JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]
        public string Currency;

        [JsonProperty("gross", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Gross;
    }

}
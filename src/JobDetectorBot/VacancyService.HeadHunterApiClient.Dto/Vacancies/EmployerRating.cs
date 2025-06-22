using Newtonsoft.Json; 
namespace VacancyService.HeadHunterApiClient.Dto{ 

    public class EmployerRating
    {
        [JsonProperty("total_rating", NullValueHandling = NullValueHandling.Ignore)]
        public string TotalRating;

        [JsonProperty("reviews_count", NullValueHandling = NullValueHandling.Ignore)]
        public int? ReviewsCount;
    }

}
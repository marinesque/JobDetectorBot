using Newtonsoft.Json; 
namespace VacancyService.HeadHunterApiClient.Dto{ 

    public class Employer
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name;

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url;

        [JsonProperty("alternate_url", NullValueHandling = NullValueHandling.Ignore)]
        public string AlternateUrl;

        [JsonProperty("logo_urls", NullValueHandling = NullValueHandling.Ignore)]
        public LogoUrls LogoUrls;

        [JsonProperty("vacancies_url", NullValueHandling = NullValueHandling.Ignore)]
        public string VacanciesUrl;

        [JsonProperty("accredited_it_employer", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AccreditedItEmployer;

        [JsonProperty("employer_rating", NullValueHandling = NullValueHandling.Ignore)]
        public EmployerRating EmployerRating;

        [JsonProperty("trusted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Trusted;
    }

}
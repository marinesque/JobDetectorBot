using Newtonsoft.Json; 
using System.Collections.Generic; 
namespace VacancyService.HeadHunterApiClient.Dto{ 

    public class Root
    {
        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public List<Item> Items;

        [JsonProperty("found", NullValueHandling = NullValueHandling.Ignore)]
        public int? Found;

        [JsonProperty("pages", NullValueHandling = NullValueHandling.Ignore)]
        public int? Pages;

        [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]
        public int? Page;

        [JsonProperty("per_page", NullValueHandling = NullValueHandling.Ignore)]
        public int? PerPage;

        [JsonProperty("clusters", NullValueHandling = NullValueHandling.Ignore)]
        public object Clusters;

        [JsonProperty("arguments", NullValueHandling = NullValueHandling.Ignore)]
        public object Arguments;

        [JsonProperty("fixes", NullValueHandling = NullValueHandling.Ignore)]
        public object Fixes;

        [JsonProperty("suggests", NullValueHandling = NullValueHandling.Ignore)]
        public object Suggests;

        [JsonProperty("alternate_url", NullValueHandling = NullValueHandling.Ignore)]
        public string AlternateUrl;
    }

}
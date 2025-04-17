using Newtonsoft.Json; 
using System.Collections.Generic; 
namespace VacancyService.HeadHunterApiClient.Dto{ 

    public class Address
    {
        [JsonProperty("city", NullValueHandling = NullValueHandling.Ignore)]
        public string City;

        [JsonProperty("street", NullValueHandling = NullValueHandling.Ignore)]
        public string Street;

        [JsonProperty("building", NullValueHandling = NullValueHandling.Ignore)]
        public string Building;

        [JsonProperty("lat", NullValueHandling = NullValueHandling.Ignore)]
        public double? Lat;

        [JsonProperty("lng", NullValueHandling = NullValueHandling.Ignore)]
        public double? Lng;

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public object Description;

        [JsonProperty("raw", NullValueHandling = NullValueHandling.Ignore)]
        public string Raw;

        [JsonProperty("metro", NullValueHandling = NullValueHandling.Ignore)]
        public Metro Metro;

        [JsonProperty("metro_stations", NullValueHandling = NullValueHandling.Ignore)]
        public List<MetroStation> MetroStations;

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;
    }

}
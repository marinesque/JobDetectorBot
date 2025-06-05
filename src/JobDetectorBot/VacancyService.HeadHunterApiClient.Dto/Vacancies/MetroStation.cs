using Newtonsoft.Json; 
namespace VacancyService.HeadHunterApiClient.Dto{ 

    public class MetroStation
    {
        [JsonProperty("station_name", NullValueHandling = NullValueHandling.Ignore)]
        public string StationName;

        [JsonProperty("line_name", NullValueHandling = NullValueHandling.Ignore)]
        public string LineName;

        [JsonProperty("station_id", NullValueHandling = NullValueHandling.Ignore)]
        public string StationId;

        [JsonProperty("line_id", NullValueHandling = NullValueHandling.Ignore)]
        public string LineId;

        [JsonProperty("lat", NullValueHandling = NullValueHandling.Ignore)]
        public double? Lat;

        [JsonProperty("lng", NullValueHandling = NullValueHandling.Ignore)]
        public double? Lng;
    }

}
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyService.DataAccess.Model
{
	public class Metro
	{
		[BsonElement("stationName")]
		public string StationName { get; set; }

		[BsonElement("lineName")]
		public string LineName { get; set; }

		[BsonElement("stationId")]
		public string StationId { get; set; }

		[BsonElement("lineId")]
		public string LineId { get; set; }

		[BsonElement("lat")]
		public double? Lat { get; set; }

		[BsonElement("lng")]
		public double? Lng { get; set; }
	}
}

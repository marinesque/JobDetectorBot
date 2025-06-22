using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyService.DataAccess.Model
{
	public class Address
	{
		[BsonElement("id")]
		public string Id { get; set; }

		[BsonElement("city")]
		public string City { get; set; }

		[BsonElement("street")]
		public string Street { get; set; }

		[BsonElement("building")]
		public string Building { get; set; }

		[BsonElement("lat")]
		public double? Lat { get; set; }

		[BsonElement("lng")]
		public double? Lng { get; set; }

		[BsonElement("description")]
		public object Description { get; set; }

		[BsonElement("raw")]
		public string Raw { get; set; }

		[BsonElement("metro")]
		public Metro Metro { get; set; }

		[BsonElement("metroStations")]
		public List<MetroStation> MetroStations { get; set; }
	}
}

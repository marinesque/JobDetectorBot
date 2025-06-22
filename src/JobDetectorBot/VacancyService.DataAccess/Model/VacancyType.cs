using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyService.DataAccess.Model
{
	public class VacancyType
	{
		[BsonElement("id")]
		public string Id;

		[BsonElement("name")]
		public string Name;
	}
}

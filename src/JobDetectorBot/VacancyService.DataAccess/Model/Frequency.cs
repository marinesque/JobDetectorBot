
using MongoDB.Bson.Serialization.Attributes;

namespace VacancyService.DataAccess.Model
{ 

    public class Frequency
    {
		[BsonElement("id")]
		public string Id;

		[BsonElement("name")]
		public string Name;
	}

}
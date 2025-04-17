
using MongoDB.Bson.Serialization.Attributes;

namespace VacancyService.DataAccess.Model
{ 

    public class Mode
    {
		[BsonElement("id")]
        public string Id;

		[BsonElement("name")]
        public string Name;
    }

}
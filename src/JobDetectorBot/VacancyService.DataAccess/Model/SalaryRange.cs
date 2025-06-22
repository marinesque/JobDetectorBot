using MongoDB.Bson.Serialization.Attributes;

namespace VacancyService.DataAccess.Model
{ 

    public class SalaryRange
    {
		[BsonElement("from")]
        public int? From;

		[BsonElement("to")]
        public int? To;

		[BsonElement("currency")]
        public string Currency;

		[BsonElement("gross")]
        public bool? Gross;

		[BsonElement("mode")]
        public Mode Mode;

		[BsonElement("frequency")]
        public Frequency? Frequency;
    }

}
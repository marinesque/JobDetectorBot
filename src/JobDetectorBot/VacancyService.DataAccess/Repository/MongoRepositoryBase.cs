using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyService.DataAccess.Repository
{
	public abstract class MongoRepositoryBase<T> where T : class
	{
		protected readonly IMongoCollection<T> Collection;

		protected MongoRepositoryBase(string collectionName, string connectionString)
		{
			MongoClientSettings settings = MongoClientSettings.FromConnectionString(connectionString);

			var client = new MongoClient(settings);
			MongoUrl mongoUrl = MongoUrl.Create(connectionString);
			IMongoDatabase database = client.GetDatabase(mongoUrl.DatabaseName);
			Collection = database.GetCollection<T>(collectionName);
		}
	}
}

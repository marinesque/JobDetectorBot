using VacancyService.DataAccess.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VacancyService.Configuration;
using MongoDB.Driver;

namespace VacancyService.DataAccess.Repository
{
	public class VacancyRepository: MongoRepositoryBase<Vacancy>, IVacancyRepository
	{
		public VacancyRepository(IOptions<MongoDBSettings> options) : base (options.Value.DatabaseName, options.Value.ConnectionString)
		{
		}

		public async Task AddAsync(Vacancy entity, CancellationToken token = default)
		{
			await Collection.InsertOneAsync(entity, cancellationToken: token);
		}

		public async Task BulkInsertAsync(List<Vacancy> entities, CancellationToken token = default)
		{
			await Collection.InsertManyAsync(entities, cancellationToken: token);
		}

		public async Task DeleteAsync(string id, CancellationToken token = default)
		{
			await Collection.DeleteOneAsync(Builders<Vacancy>.Filter.Eq(x => x.Id, id), cancellationToken: token);
		}

		public async Task<List<Vacancy>> GetAllAsync(CancellationToken token)
		{
			return await Collection.Find(Builders<Vacancy>.Filter.Empty).ToListAsync(token);
		}

		public async Task<List<Vacancy>> GetAllBySearchString(string search, DbSearchOptions dbSearchOptions, CancellationToken token = default)
		{
			var builder = Builders<Vacancy>.Filter;
			var filter = builder.Empty;

			if(string.IsNullOrEmpty(search) || string.IsNullOrWhiteSpace(search))
			{
				throw new ArgumentNullException(nameof(search));
			}

			filter &= builder.Text(search);

			if(dbSearchOptions?.Salary.HasValue == true)
			{
				filter &= builder.And(builder.Gte(x => x.Salary.From, dbSearchOptions.Salary), builder.Lte(x => x.Salary.To, dbSearchOptions.Salary));
			}
			if (!string.IsNullOrEmpty(dbSearchOptions.SalaryRangeFrequency))
			{
				filter &= builder.Eq(x => x.Salary.Frequency.Id, dbSearchOptions.SalaryRangeFrequency);
			}
			if (!string.IsNullOrEmpty(dbSearchOptions.Schedule))
			{
				filter &= builder.Eq(x => x.Schedule.Name, dbSearchOptions.Schedule);
			}
			if (!string.IsNullOrEmpty(dbSearchOptions.Employment))
			{
				filter &= builder.Eq(x => x.Employment.Name, dbSearchOptions.Employment);
			}

			List<Vacancy> result = await Collection.Find(filter).ToListAsync();

			return result;
		}

		public async Task<List<Vacancy>> GetAllBySearchString(List<string> vacancyNames, DbSearchOptions dbSearchOptions, CancellationToken token = default)
		{
			var builder = Builders<Vacancy>.Filter;
			var filter = builder.Empty;

			if (!vacancyNames.Any())
			{
				throw new ArgumentNullException(nameof(vacancyNames));
			}

			string vacanciesString = string.Join(' ', vacancyNames.ToArray());

			filter &= builder.Text(vacanciesString);

			DateTime to_date = DateTime.Now;
			DateTime from_date = to_date.AddDays(-3);

			filter &= builder.And(builder.Gte(x => x.PublishedVacancyDate, from_date), builder.Lte(x => x.PublishedVacancyDate, to_date));

			filter &= builder.Eq(x => x.Archived, false);

			if (dbSearchOptions?.Salary.HasValue == true)
			{
				filter &= builder.And(builder.Gte(x => x.Salary.From, dbSearchOptions.Salary), builder.Lte(x => x.Salary.To, dbSearchOptions.Salary));
			}
			if (!string.IsNullOrEmpty(dbSearchOptions.SalaryRangeFrequency))
			{
				filter &= builder.Eq(x => x.Salary.Frequency.Id, dbSearchOptions.SalaryRangeFrequency);
			}
			if (!string.IsNullOrEmpty(dbSearchOptions.Schedule))
			{
				filter &= builder.Eq(x => x.Schedule.Name, dbSearchOptions.Schedule);
			}
			if (!string.IsNullOrEmpty(dbSearchOptions.Employment))
			{
				filter &= builder.Eq(x => x.Employment.Name, dbSearchOptions.Employment);
			}

			List<Vacancy> result = await Collection.Find(filter).ToListAsync();

			return result;
		}

		public async Task<Vacancy?> GetByIdAsync(string id, CancellationToken token)
		{
			return await Collection.Find(Builders<Vacancy>.Filter.Eq(x => x.Id, id)).FirstOrDefaultAsync(token);
		}

		public async Task UpdateAsync(string id, Vacancy entity, CancellationToken token = default)
		{
			var updateDefBuilder = new UpdateDefinitionBuilder<Vacancy>();

			var updateDefinitions = new List<UpdateDefinition<Vacancy>>
			{
				updateDefBuilder.Set(x=>x.Requirement, entity.Requirement),
				updateDefBuilder.Set(x=>x.Schedule, entity.Schedule),
				updateDefBuilder.Set(x=>x.Salary, entity.Salary),
				updateDefBuilder.Set(x=>x.Employer, entity.Employer),
				updateDefBuilder.Set(x=>x.Employment, entity.Employment),
				updateDefBuilder.Set(x=>x.EmploymentForm, entity.EmploymentForm),
				updateDefBuilder.Set(x=>x.Name, entity.Name),
				updateDefBuilder.Set(x=>x.WorkExperience, entity.WorkExperience),
				updateDefBuilder.Set(x=>x.WorkFormat, entity.WorkFormat),
				updateDefBuilder.Set(x=>x.WorkingHours, entity.WorkingHours),
				updateDefBuilder.Set(x=>x.WorkScheduleByDays, entity.WorkScheduleByDays),
			};

			FilterDefinition<Vacancy> filter = Builders<Vacancy>.Filter.Eq(x => x.Id, id);

			UpdateResult ur = await Collection.UpdateOneAsync(filter, updateDefBuilder.Combine(updateDefinitions), cancellationToken: token);

			if (ur == null || ur.IsAcknowledged && ur.MatchedCount < 1)
				throw new Exception("Не удалось обновить сущность");

		}
	}
}

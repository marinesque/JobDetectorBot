using VacancyService.DataAccess.Context;
using VacancyService.DataAccess.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VacancyService.Configuration;
using MongoDB.Driver;

namespace VacancyService.DataAccess.Repository
{
	public class VacancyRepository: MongoRepositoryBase<Vacancy>, IVacancyRepository
	{
		private readonly VacancyDbContext _context;

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

		public async Task DeleteAsync(Guid id, CancellationToken token = default)
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

			if(dbSearchOptions?.Salary.HasValue == true)
			{
				filter &= builder.And(builder.Gte(x => x.Salary.From, dbSearchOptions.Salary), builder.Lte(x => x.Salary.To, dbSearchOptions.Salary));
			}
			if(!string.IsNullOrEmpty(dbSearchOptions.Schedule))
			{
				filter &= builder.Eq(x => x.Schedule, dbSearchOptions.Schedule);
			}
			if(dbSearchOptions.)

			Collection.Find(Builders<Vacancy>.Filter.Eq(x=> x.Salary.From => dbSearchOptions.Salary))


			var result = _context.Vacancies.Where(x => x.Name.ToLower() == search.ToLower());

			if (dbSearchOptions.Salary.HasValue)
				result = result.Where(x=> dbSearchOptions.Salary <= x.Salary.To && dbSearchOptions.Salary >= x.Salary.From);

			//if ( !string.IsNullOrEmpty(dbSearchOptions.Experience))
			//	result = result.Where(x => x.WorkExperience.Id == dbSearchOptions.Experience);

			if (!string.IsNullOrEmpty(dbSearchOptions.Schedule))
				result = result.Where(x => x.Schedule == dbSearchOptions.Schedule);

			//if (!string.IsNullOrEmpty(dbSearchOptions.SalaryRangeFrequency))
			//	result = result.Where(x => x.Salary.Frequency.Id == dbSearchOptions.SalaryRangeFrequency);

			if (!string.IsNullOrEmpty(dbSearchOptions.Employment))
				result = result.Where(x => x.Employment == dbSearchOptions.Employment);

			return new List<Vacancy>();



		}

		public async Task<Vacancy?> GetByIdAsync(Guid id, CancellationToken token)
		{
			return await Collection.Find(Builders<Vacancy>.Filter.Eq(x => x.Id, id)).FirstOrDefaultAsync(token);
		}

		public async Task UpdateAsync(Guid id, Vacancy entity, CancellationToken token = default)
		{
			var updateDefBuilder = new UpdateDefinitionBuilder<Vacancy>();

			var updateDefinitions = new List<UpdateDefinition<Vacancy>>
			{
				updateDefBuilder.Set(x=>x.Requirement, entity.Requirement),
				updateDefBuilder.Set(x=>x.Schedule, entity.Schedule),
				updateDefBuilder.Set(x=>x.Salary, entity.Salary),
				updateDefBuilder.Set(x=>x.Company, entity.Company),
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

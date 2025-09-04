using VacancyService.DataAccess.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VacancyService.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
using MongoDB.Driver.Linq;

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
			int pageSize = 10;

			if(string.IsNullOrEmpty(search) || string.IsNullOrWhiteSpace(search))
			{
				throw new ArgumentNullException(nameof(search));
			}

			filter &= builder.Text(search);

			DateTime to_date = DateTime.Now;
			DateTime from_date = to_date.AddDays(-3);

			filter &= builder.And(builder.Gte(x => x.PublishedVacancyDate, from_date), builder.Lte(x => x.PublishedVacancyDate, to_date));

			filter &= builder.Eq(x => x.Archived, false);

			if (dbSearchOptions?.Salary.HasValue == true)
			{
				filter &= builder.And(builder.Gte(x => x.Salary.From, dbSearchOptions.Salary), builder.Lt(x => x.Salary.To, dbSearchOptions.Salary));
			}
			if (!string.IsNullOrEmpty(dbSearchOptions.SalaryRangeFrequency))
			{
				filter &= builder.Where(x => x.Salary != null && x.Salary.Frequency != null && x.Salary.Frequency.Id == dbSearchOptions.SalaryRangeFrequency);
			}
			if (!string.IsNullOrEmpty(dbSearchOptions.Schedule))
			{
				filter &= builder.Where(x => x.Schedule != null && x.Schedule.Id == dbSearchOptions.Schedule);
			}
			if (!string.IsNullOrEmpty(dbSearchOptions.Employment))
			{
				filter &= builder.Where(x => x.Employment != null && x.Employment.Id == dbSearchOptions.Employment);
			}
			if (!string.IsNullOrEmpty(dbSearchOptions.Employment))
			{
				filter &= builder.Eq(x => x.Employment.Name, dbSearchOptions.Employment);
			}

			if (!string.IsNullOrEmpty(dbSearchOptions.Region))
			{
				filter &= builder.Eq(x => x.Area.Id, dbSearchOptions.Region);
			}


			List<Vacancy> result;
			var sortDefinition = Builders<Vacancy>.Sort.Descending(doc => doc.ExternalId);

			if (dbSearchOptions.Page.HasValue)
			{
				result = await Collection.Find(filter).Sort(sortDefinition).Skip(dbSearchOptions.Page.Value * pageSize).Limit(pageSize).ToListAsync();
			}
			else
			{
				result = await Collection.Find(filter).Sort(sortDefinition).ToListAsync();
			}

			return result;
		}

		public async Task<List<Vacancy>> GetAllBySearchString(Dictionary<string, List<string>> vacancyNames, DbSearchOptions dbSearchOptions, CancellationToken token = default)
		{
			try
			{
				var builder = Builders<Vacancy>.Filter;
				var filter = builder.Empty;
				int pageSize = 10;

				if (!vacancyNames.Any())
				{
					throw new ArgumentNullException(nameof(vacancyNames));
				}

				string vacanciesString = string.Join(' ', vacancyNames.Keys.ToArray());

				filter &= builder.Text(vacanciesString);
				vacancyNames.Values.Select(s => s.Select(x => x)).ToList();
				var profRoles = vacancyNames.SelectMany(s => s.Value).Distinct().ToList();

				filter &= builder.AnyIn(x => x.ProfessionalRoles.Select(s => s.Id), profRoles);

				DateTime to_date = DateTime.Now;
				DateTime from_date = to_date.AddDays(-3);

				filter &= builder.And(builder.Gte(x => x.PublishedVacancyDate, from_date), builder.Lte(x => x.PublishedVacancyDate, to_date));

				filter &= builder.Eq(x => x.Archived, false);

				if (dbSearchOptions?.Salary.HasValue == true)
				{
					// Фильтр для случаев, когда From и To могут быть null
					var salaryValue = dbSearchOptions.Salary.Value;
					var salaryFilter = builder.Or(
						// Диапазон: From и To заданы
						builder.And(
							builder.Where(x => x.Salary.From != null && x.Salary.To != null),
							builder.Lte(x => x.Salary.From, salaryValue),
							builder.Gte(x => x.Salary.To, salaryValue)
						),
						// Только From задано
						builder.And(
							builder.Where(x => x.Salary.From != null && x.Salary.To == null),
							builder.Lte(x => x.Salary.From, salaryValue)
						),
						// Только To задано
						builder.And(
							builder.Where(x => x.Salary.From == null && x.Salary.To != null),
							builder.Gte(x => x.Salary.To, salaryValue)
						)
					);
					filter &= salaryFilter;
				}
				if (!string.IsNullOrEmpty(dbSearchOptions.SalaryRangeFrequency))
				{
					filter &= builder.Where(x => x.Salary != null && x.Salary.Frequency != null && x.Salary.Frequency.Id == dbSearchOptions.SalaryRangeFrequency);
				}
				if (!string.IsNullOrEmpty(dbSearchOptions.Schedule))
				{
					filter &= builder.Eq(x => x.Schedule.Id, dbSearchOptions.Schedule);
				}
				if (!string.IsNullOrEmpty(dbSearchOptions.Employment))
				{
					filter &= builder.Eq(x => x.Employment.Id, dbSearchOptions.Employment);
				}

				if (!string.IsNullOrEmpty(dbSearchOptions.Region))
				{
					filter &= builder.Eq(x => x.Area.Id, dbSearchOptions.Region);
				}

				List<Vacancy> result;
				var sortDefinition = Builders<Vacancy>.Sort.Descending(doc => doc.ExternalId);

				if (dbSearchOptions.Page.HasValue)
				{
					result = await Collection.Find(filter).Sort(sortDefinition).Skip(dbSearchOptions.Page.Value * pageSize).Limit(pageSize).ToListAsync();
				}
				else
				{
					result = await Collection.Find(filter).Sort(sortDefinition).ToListAsync();
				}

				return result;
			}
			catch (MongoCommandException ex)
			{
				return new List<Vacancy>();
			}

		}

		public async Task<List<string>> GetAllByExternalIdsAsync(List<string> externalIds, CancellationToken token = default)
		{
			if (externalIds == null || externalIds.Count == 0)
				return new List<string>();

			var filter = Builders<Vacancy>.Filter.In(x => x.ExternalId, externalIds);
			var existingVacancies = await Collection.Find(filter).Project(x => x.ExternalId).ToListAsync(token);
			return existingVacancies;
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

using VacancyService.DataAccess.Context;
using VacancyService.DataAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace VacancyService.DataAccess.Repository
{
	public class VacancyRepository: IRepository<Vacancy>
	{
		private readonly VacancyDbContext _context;

		public VacancyRepository(VacancyDbContext context)
		{
			_context = context;
			_context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
		}

		public async Task AddAsync(Vacancy entity)
		{
			_context.Vacancies.Add(entity);
			await _context.SaveChangesAsync();
		}

		public async Task BulkInsertAsync(List<Vacancy> entities)
		{			
			foreach (var entityItem in entities)
			{
				_context.Vacancies.Add(entityItem);
			}
			await _context.SaveChangesAsync();
		}

		public async Task DeleteAsync(Guid id)
		{
			var entity = await _context.Vacancies.Where(x => x.Id == id).FirstOrDefaultAsync();

			if (entity != null)
			{
				_context.Vacancies.Remove(entity);
				_context.ChangeTracker.DetectChanges();
				await _context.SaveChangesAsync();
			}
			else
			{
				throw new ArgumentException($"Запись с Id {id} отсутствует");
			}
		}

		public async Task<List<Vacancy>> GetAllAsync()
		{
			return await _context.Vacancies.ToListAsync();
		}

		public async Task<List<Vacancy>> GetAllBySearchString(string search, DbSearchOptions dbSearchOptions)
		{
			var result = _context.Vacancies.Where(x => EF.Functions.Like(x.Name.ToLower(), search.ToLower()));

			if (dbSearchOptions.Salary.HasValue)
				result = result.Where(x=> dbSearchOptions.Salary <= x.Salary.To && dbSearchOptions.Salary >= x.Salary.From);

			if ( !string.IsNullOrEmpty(dbSearchOptions.Experience))
				result = result.Where(x => x.WorkExperience.Id == dbSearchOptions.Experience);

			if (!string.IsNullOrEmpty(dbSearchOptions.Schedule))
				result = result.Where(x => x.Schedule == dbSearchOptions.Schedule);

			if (!string.IsNullOrEmpty(dbSearchOptions.SalaryRangeFrequency))
				result = result.Where(x => x.Salary.Frequency.Id == dbSearchOptions.SalaryRangeFrequency);

			if (!string.IsNullOrEmpty(dbSearchOptions.Employment))
				result = result.Where(x => x.Employment == dbSearchOptions.Employment);

			return await result.ToListAsync();

		}

		public async Task<Vacancy?> GetByIdAsync(Guid id)
		{
			return await _context.Vacancies.FirstOrDefaultAsync(x => x.Id == id);
		}

		public async Task UpdateAsync(Guid id, Vacancy entity)
		{
			Vacancy? entityToUpdate = await _context.Vacancies.Where(x => x.Id == id).FirstOrDefaultAsync();

			if (entity != null)
			{
				entityToUpdate.Name = entity.Name;
				entityToUpdate.Requirement = entity.Requirement;
				entityToUpdate.Responsibility = entity.Responsibility;
				entityToUpdate.Schedule = entity.Schedule;
				entityToUpdate.WorkingHours = entity.WorkingHours;
				entityToUpdate.Salary = entity.Salary;
				entityToUpdate.WorkExperience = entity.WorkExperience;
				entityToUpdate.Employment = entity.Employment;
				entityToUpdate.EmploymentForm = entity.EmploymentForm;
				entityToUpdate.WorkScheduleByDays = entity.WorkScheduleByDays;
				entityToUpdate.CreatedVacancyDate = entity.CreatedVacancyDate;
				entityToUpdate.Company = entity.Company;
				entityToUpdate.Link = entity.Link;

				_context.Vacancies.Update(entityToUpdate);

				_context.ChangeTracker.DetectChanges();
				await _context.SaveChangesAsync();
			}
			else
			{
				throw new ArgumentException($"Запись с Id {id} отсутствует");
			}
		}
	}
}

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
		}

		public async Task AddAsync(Vacancy entity)
		{
			_context.Vacancies.Add(entity);
			_context.ChangeTracker.DetectChanges();
			await _context.SaveChangesAsync();
		}

		public async Task BulkInsertAsync(List<Vacancy> entity)
		{
			_context.Vacancies.AddRange(entity);
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

		public async Task<List<Vacancy>> GetAllBySearchString(string search)
		{
			return await _context.Vacancies.Where(x => EF.Functions.Like(x.Name, "%search%") || EF.Functions.Like(x.Description, "%search%") || EF.Functions.Like(x.Company, "%search%")).ToListAsync();
		}

		public async Task<List<Vacancy>> GetAllBySearchString(string search, SearchKeys keys)
		{
			switch(keys)
			{
				case SearchKeys.Name:
					return await _context.Vacancies.Where(x => EF.Functions.Like(x.Name, "%search%")).ToListAsync();
				case SearchKeys.Name | SearchKeys.Company:
					return await _context.Vacancies.Where(x => EF.Functions.Like(x.Name, "%search%") || EF.Functions.Like(x.Company, "%search%")).ToListAsync();
				case SearchKeys.Name | SearchKeys.Description:
					return await _context.Vacancies.Where(x => EF.Functions.Like(x.Name, "%search%") || EF.Functions.Like(x.Description, "%search%")).ToListAsync();
				case SearchKeys.Company | SearchKeys.Description:
					return await _context.Vacancies.Where(x => EF.Functions.Like(x.Company, "%search%") || EF.Functions.Like(x.Description, "%search%")).ToListAsync();
				case SearchKeys.Name | SearchKeys.Company | SearchKeys.Description:
				default:
					return await _context.Vacancies
						.Where(x => EF.Functions.Like(x.Name, "%search%") || EF.Functions.Like(x.Company, "%search%") || EF.Functions.Like(x.Description, "%search%"))
						.ToListAsync();
			}

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
				entityToUpdate.Description = entity.Description;
				entityToUpdate.Schedule = entity.Schedule;
				entityToUpdate.WorkTime = entity.WorkTime;
				entityToUpdate.Salary = entity.Salary;
				entityToUpdate.WorkExperience = entity.WorkExperience;
				entityToUpdate.Job = entity.Job;
				entityToUpdate.Salary = entity.Salary;
				entityToUpdate.WorkType = entity.WorkType;
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

using HeadHunterGrabber.DataAccess.Context;
using HeadHunterGrabber.DataAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace HeadHunterGrabber.DataAccess.Repository
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
				entityToUpdate.WorkHours = entity.WorkHours;
				entityToUpdate.Salary = entity.Salary;
				entityToUpdate.WorkExperience = entity.WorkExperience;
				entityToUpdate.Job = entity.Job;
				entityToUpdate.Salary = entity.Salary;
				entityToUpdate.Type = entity.Type;

				_context.Vacancies.Update(entityToUpdate);

				_context.ChangeTracker.DetectChanges();
				await _context.SaveChangesAsync();
			}
			else
			{
				throw new ArgumentException($"Запись с Id {id} отсутствует");
			}
		}

		//public async Task AddAsync(T entity)
		//{
		//	await _collection.InsertOneAsync(entity);
		//}

		//public async Task UpdateAsync(string id, T entity)
		//{
		//	await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", new ObjectId(id)), entity);
		//}

		//public async Task DeleteAsync(string id)
		//{
		//	await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", new ObjectId(id)));
		//}

		//public async Task<List<T>> GetAllAsync()
		//{
		//	return await _collection.Find(new BsonDocument()).ToListAsync();
		//}

		//public async Task<T> GetByIdAsync(string id)
		//{
		//	return await _collection.Find(Builders<T>.Filter.Eq("_id", new ObjectId(id))).FirstOrDefaultAsync();
		//}

	}
}

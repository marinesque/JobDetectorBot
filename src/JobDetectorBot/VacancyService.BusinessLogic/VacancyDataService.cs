using System.Xml.Linq;
using VacancyService.DataAccess.Model;
using VacancyService.DataAccess.Repository;
using VacancyService.Dto;
using VacancyService.Parser;
using VacancyService.Parser.Dto;

namespace VacancyService.BusinessLogic
{
	public class VacancyDataService : IVacancyDataService
	{

		private readonly IRepository<Vacancy> _vacancyRepository;
		private readonly IParser<SiteVacancy, SiteSearchParam> _parser;

		public VacancyDataService(IRepository<Vacancy> vacancyRepository,
			IParser<SiteVacancy, SiteSearchParam> parser)
		{
			_vacancyRepository = vacancyRepository;
			_parser = parser;
		}

		public async Task AddAsync(VacancyCreateRequest dto)
		{
			Vacancy entity = new Vacancy
			{
				Id = new Guid(),
				Name = dto.Name,
				Salary = dto.Salary,
				Description = dto.Description,
				Job = dto.Job,
				Schedule = dto.Schedule,
				WorkType = dto.Type,
				WorkExperience = dto.WorkExperience,
				WorkTime = dto.WorkHours
			};

			await _vacancyRepository.AddAsync(entity);
		}

		public async Task DeleteAsync(Guid id)
		{
			await _vacancyRepository.DeleteAsync(id);
		}

		public async Task<List<VacancyResponse>> FindVacancy(string search, bool name, bool company, bool description)
		{
			//List<Vacancy> vacanciesInDb = await _vacancyRepository.GetAllBySearchString(search, MapSearchKey(name, company, description));

			//if(vacanciesInDb.Any())
			//{
			//	//var dataToInsert = await _parser.GetVacancies(new SiteSearchParam { SearchString = search, SearchKeyWords = Map(searchKeys) });

			//	return vacanciesInDb.Select(x=> new VacancyResponse
			//	{
			//		Company = x.Company,
			//		CreatedVacancyDate = x.CreatedVacancyDate,
			//		Description = x.Description,
			//		Link = x.Link,
			//		Name = x.Name,
			//		Salary= x.Salary,
			//		Schedule = x.Schedule,
			//		WorkExperience= x.WorkExperience,
			//		WorkFormat = x.WorkFormat,
			//		WorkTime = x.WorkTime,
			//		WorkType = x.WorkType
			//	}).ToList();
			//}

			var results = await _parser.GetVacancies(new SiteSearchParam { SearchString = search, SearchKeyWords = Map(name, company, description) });

			//await _vacancyRepository.BulkInsertAsync(results.Select(x=> new Vacancy
			//{
			//	Company = x.Company,
			//	CreatedVacancyDate= x.CreatedVacancyDate,
			//	Description = x.Description,
			//	Job = x.Job,
			//	Link = x.Link,
			//	Name = x.Name,
			//	Salary= x.Salary,
			//	Schedule = x.Schedule,
			//	WorkExperience= x.WorkExperience,
			//	WorkFormat= x.WorkFormat,
			//	WorkTime = x.WorkTime,
			//	WorkType = x.WorkType,
			//	Id = Guid.NewGuid(),
			//	Create = DateTime.Now
			//}).ToList());

			//List<Vacancy> vacanciesInDb = await _vacancyRepository.GetAllByName(name);
			//if(!vacanciesInDb.Any())
			//{
			//	//TODO: Идем в парсер и достаем все по названию
			//}

			//return vacanciesInDb.Select(x => new VacancyResponse
			//{
			//	Description = x.Description,
			//	Job = x.Job,
			//	Name = x.Name,
			//	Salary = x.Salary,
			//	Schedule = x.Schedule,
			//	Type = x.Type,
			//	WorkExperience = x.WorkExperience,
			//	WorkHours = x.WorkHours
			//}).ToList();
			return results.Select(x=>new VacancyResponse
			{
				Name = x.Name,
				Company= x.Company,
				CreatedVacancyDate = x.CreatedVacancyDate,
				Job= x.Job,
				Link = x.Link,
				Salary = x.Salary,
				Schedule = x.Schedule,
				WorkExperience= x.WorkExperience,
				WorkFormat = x.WorkFormat,
				WorkTime = x.WorkTime,
				WorkType = x.WorkType,
				Description = x.Description
			}).ToList();
		}

		

		private DataAccess.SearchKeys MapSearchKey(bool name, bool company, bool description)
		{
			if(name && company && description)
				return DataAccess.SearchKeys.Name | DataAccess.SearchKeys.Company | DataAccess.SearchKeys.Description;

			if (name && company)
				return DataAccess.SearchKeys.Name | DataAccess.SearchKeys.Company;

			if (name && description)
				return DataAccess.SearchKeys.Name | DataAccess.SearchKeys.Description;

			if (company && description)
				return DataAccess.SearchKeys.Name | DataAccess.SearchKeys.Description;

			if(company)
				return DataAccess.SearchKeys.Company;

			if(description)
				return DataAccess.SearchKeys.Description;

			return DataAccess.SearchKeys.Name;
		}

		private SearchKeyWords Map(bool name, bool company, bool description)
		{
			if (name && company && description)
				return SearchKeyWords.Name | SearchKeyWords.Company | SearchKeyWords.Description;

			if (name && company)
				return SearchKeyWords.Name | SearchKeyWords.Company;

			if (name && description)
				return SearchKeyWords.Name | SearchKeyWords.Description;

			if (company && description)
				return SearchKeyWords.Name | SearchKeyWords.Description;

			if (company)
				return SearchKeyWords.Company;

			if (description)
				return SearchKeyWords.Description;

			return SearchKeyWords.Name;
		}

		public async Task<List<VacancyResponse>> GetAllAsync()
		{
			List<Vacancy> vacancies = await _vacancyRepository.GetAllAsync();
			return vacancies.Select(x=> new VacancyResponse
			{
				Description = x.Description,
				Job = x.Job,
				Name = x.Name,
				Salary = x.Salary,
				Schedule = x.Schedule,
				WorkType = x.WorkType,
				WorkExperience= x.WorkExperience,
				WorkTime = x.WorkTime,
				Company = x.Company,
				CreatedVacancyDate = x.CreatedVacancyDate,
				Link = x.Link,
				WorkFormat = x.WorkFormat
			}).ToList();
		}

		public async Task<VacancyResponse> GetByIdAsync(Guid id)
		{
			Vacancy vacancy = await _vacancyRepository.GetByIdAsync(id);
			return new VacancyResponse
			{
				Description= vacancy.Description,
				Job = vacancy.Job,
				Name = vacancy.Name,
				Salary= vacancy.Salary,
				Schedule = vacancy.Schedule,
				WorkType = vacancy.WorkType,
				WorkExperience = vacancy.WorkExperience,
				WorkTime = vacancy.WorkTime,
				Company = vacancy.Company,
				CreatedVacancyDate = vacancy.CreatedVacancyDate,
				WorkFormat= vacancy.WorkFormat,
				Link = vacancy.Link
			};
		}

		public async Task UpdateAsync(Guid id, VacancyCreateRequest dto)
		{
			await _vacancyRepository.UpdateAsync(id, new Vacancy
			{
				Description = dto.Description,
				WorkTime= dto.WorkHours,
				Id = id,
				Job = dto.Job,
				Name = dto.Name,
				Salary= dto.Salary,
				Schedule = dto.Schedule,
				WorkType = dto.Type,
				WorkExperience = dto.WorkExperience
			});
		}
	}
}

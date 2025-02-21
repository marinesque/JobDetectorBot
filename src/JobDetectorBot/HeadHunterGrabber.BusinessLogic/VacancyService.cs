using HeadHunterGrabber.DataAccess.Model;
using HeadHunterGrabber.DataAccess.Repository;
using HeadHunterGrabber.Dto;
using HeadHunterGrabber.Parser;
using HeadHunterGrabber.Parser.Dto;
using SharpCompress.Common;

namespace HeadHunterGrabber.BusinessLogic
{
	public class VacancyService : IVacancyService
	{

		private readonly IRepository<Vacancy> _vacancyRepository;
		private readonly IParser<SiteVacancy, SiteSearchParam> _parser;

		public VacancyService(IRepository<Vacancy> vacancyRepository,
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
				Type = dto.Type,
				WorkExperience = dto.WorkExperience,
				WorkHours = dto.WorkHours
			};

			await _vacancyRepository.AddAsync(entity);
		}

		public async Task DeleteAsync(Guid id)
		{
			await _vacancyRepository.DeleteAsync(id);
		}

		public async Task<List<VacancyResponse>> FindVacancyByName(string name)
		{
			var result = _parser.GetVacancies(new SiteSearchParam { SearchString = name, SearchKeyWords = SearchKeyWords.Name });
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
			return new List<VacancyResponse>();
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
				Type = x.Type,
				WorkExperience= x.WorkExperience,
				WorkHours = x.WorkHours
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
				Type = vacancy.Type,
				WorkExperience = vacancy.WorkExperience,
				WorkHours = vacancy.WorkHours
			};
		}

		public async Task UpdateAsync(Guid id, VacancyCreateRequest dto)
		{
			await _vacancyRepository.UpdateAsync(id, new Vacancy
			{
				Description = dto.Description,
				WorkHours= dto.WorkHours,
				Id = id,
				Job = dto.Job,
				Name = dto.Name,
				Salary= dto.Salary,
				Schedule = dto.Schedule,
				Type = dto.Type,
				WorkExperience = dto.WorkExperience
			});
		}
	}
}

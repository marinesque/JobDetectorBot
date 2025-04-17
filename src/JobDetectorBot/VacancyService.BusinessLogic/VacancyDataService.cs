using MongoDB.Driver;
using System.Net.Http.Headers;
using System.Xml.Linq;
using VacancyService.DataAccess.Model;
using VacancyService.DataAccess.Repository;
using VacancyService.Dto;
using VacancyService.HeadHunterApiClient;
using VacancyService.HeadHunterApiClient.Dto;
using VacancyService.Parser;
using VacancyService.Parser.Dto;

namespace VacancyService.BusinessLogic
{
	public class VacancyDataService : IVacancyDataService
	{

		private readonly IRepository<Vacancy> _vacancyRepository;
		private readonly IParser<SiteVacancy, SiteSearchParam> _parser;
		private readonly IServiceClient _hhClient;

		public VacancyDataService(IRepository<Vacancy> vacancyRepository,
			IParser<SiteVacancy, SiteSearchParam> parser,
			IServiceClient hhClient)
		{
			_vacancyRepository = vacancyRepository;
			_parser = parser;
			_hhClient = hhClient;
		}

		public async Task DeleteAsync(Guid id)
		{
			await _vacancyRepository.DeleteAsync(id);
		}

		public async Task<List<VacancyResponse>> FindVacancy(string search, VacancySearchOptions searchOptions)
		{
			DbSearchOptions dbSearchOptions = Map(searchOptions);

			Dictionary<string, string> hhSearchOptions = CreateHeadHunterSearchOptions(searchOptions);

			List<Vacancy> vacanciesInDb = await _vacancyRepository.GetAllBySearchString(search, dbSearchOptions);

			Root hhResult;

			if (vacanciesInDb.Any())
			{

				hhResult = await _hhClient.GetVacancies(search, hhSearchOptions);

				if(vacanciesInDb.Count < hhResult.Items.Count)
				{
					TryAddNewItems(vacanciesInDb, hhResult.Items);
					vacanciesInDb = await _vacancyRepository.GetAllBySearchString(search, dbSearchOptions);
				}

				return vacanciesInDb.Select(x => new VacancyResponse
				{
					Company = x.Company,
					CreatedVacancyDate = x.CreatedVacancyDate,
					Requirement = x.Requirement,
					Responsibility = x.Responsibility,
					Employment = x.Employment,
					Link = x.Link,
					Name = x.Name,
					Salary = Map(x.Salary),
					Schedule = x.Schedule,
					WorkExperience = Map(x.WorkExperience),
					WorkFormat = x.WorkFormat,
					WorkingHours = x.WorkingHours,
					WorkScheduleByDays = x.WorkScheduleByDays,
					UniqName = x.UniqName,
				}).ToList();
			}
			else
			{
				hhResult = await _hhClient.GetVacancies(search, hhSearchOptions);
				await _vacancyRepository.BulkInsertAsync(hhResult.Items.Select(x => new Vacancy
				{
					Company = x.Employer?.Name,
					CreatedVacancyDate = x.PublishedAt,
					Requirement = x.Snippet.Requirement,
					Responsibility = x.Snippet.Responsibility,
					Employment = x.Employment?.Name,
					Link = x.AlternateUrl,
					Name = x.Name,
					Salary = Map(x.SalaryRange),
					Schedule = x.Schedule?.Name,
					WorkExperience = Map(x.Experience),
					WorkFormat = x.WorkFormat.Select(x => x.Name).ToArray(),
					WorkingHours = x.WorkingHours.Select(x => x.Name).ToArray(),
					WorkScheduleByDays = x.WorkScheduleByDays.Select(x => x.Name).ToArray(),
					Id = Guid.NewGuid(),
					UniqName = $"hh_{x.Id}",
					Create = DateTime.Now,
					EmploymentForm = x.EmploymentForm?.Name,
					ExternalId = x.Id
				}).ToList());
			}

			return hhResult.Items.Select(x => new VacancyResponse
			{
				Company = x.Employer?.Name,
				CreatedVacancyDate = x.PublishedAt,
				Requirement = x.Snippet.Requirement,
				Responsibility = x.Snippet.Responsibility,
				Employment = x.Employment?.Name,
				Link = x.AlternateUrl,
				Name = x.Name,
				Salary = MapSalary(x.SalaryRange),
				Schedule = x.Schedule?.Name,
				WorkExperience = MapWorkExperience(x.Experience),
				WorkFormat = x.WorkFormat.Select(x => x.Name).ToArray(),
				WorkingHours = x.WorkingHours.Select(x => x.Name).ToArray(),
				WorkScheduleByDays = x.WorkScheduleByDays.Select(x => x.Name).ToArray(),
				UniqName = $"hh_{x.Id}"
			}).ToList();
		}

		private async Task TryAddNewItems(List<Vacancy> vacanciesInDb, List<Item> hhItems)
		{
			var items = hhItems.Select(x => x.Id).Except(vacanciesInDb.Select(s => s.ExternalId));
			var toAdd = hhItems.Where(x => items.Contains(x.Id));
			await _vacancyRepository.BulkInsertAsync(toAdd.Select(x => new Vacancy
			{
				Company = x.Employer?.Name,
				CreatedVacancyDate = x.PublishedAt,
				Requirement = x.Snippet.Requirement,
				Responsibility = x.Snippet.Responsibility,
				Employment = x.Employment?.Name,
				Link = x.AlternateUrl,
				Name = x.Name,
				Salary = Map(x.SalaryRange),
				Schedule = x.Schedule?.Name,
				WorkExperience = Map(x.Experience),
				WorkFormat = x.WorkFormat.Select(x => x.Name).ToArray(),
				WorkingHours = x.WorkingHours.Select(x => x.Name).ToArray(),
				WorkScheduleByDays = x.WorkScheduleByDays.Select(x => x.Name).ToArray(),
				Id = Guid.NewGuid(),
				UniqName = $"hh_{x.Id}",
				Create = DateTime.Now,
				EmploymentForm = x.EmploymentForm?.Name,
				ExternalId = x.Id
			}).ToList());
		}

		private Dto.Experience MapWorkExperience(HeadHunterApiClient.Dto.Experience experience)
		{
			return new Dto.Experience
			{
				Id = experience.Id,
				Name = experience.Name
			};
		}

		private Dto.SalaryRange MapSalary(HeadHunterApiClient.Dto.SalaryRange salaryRange)
		{
			return new Dto.SalaryRange
			{
				Currency = salaryRange.Currency,
				Frequency = MapFrequency(salaryRange.Frequency),
				From = salaryRange.From,
				To = salaryRange.To,
				Gross = salaryRange.Gross,
				Mode = MapMode(salaryRange.Mode)
			};
		}

		private Dto.Mode MapMode(HeadHunterApiClient.Dto.Mode mode)
		{
			return new Dto.Mode
			{
				Name = mode.Name,
				Id = mode.Id,
			};
		}

		private Dto.Frequency MapFrequency(HeadHunterApiClient.Dto.Frequency frequency)
		{
			return new Dto.Frequency
			{
				Id = frequency.Id,
				Name = frequency.Name
			};
		}

		private DataAccess.Model.Experience Map(HeadHunterApiClient.Dto.Experience experience)
		{
			return new DataAccess.Model.Experience
			{
				Id = experience.Id,
				Name = experience.Name
			};
		}

		private DataAccess.Model.SalaryRange Map(HeadHunterApiClient.Dto.SalaryRange salaryRange)
		{
			return new DataAccess.Model.SalaryRange
			{
				Currency = salaryRange.Currency,
				Frequency = Map(salaryRange.Frequency),
				From = salaryRange.From,
				To = salaryRange.To,
				Gross = salaryRange.Gross,
				Mode = Map(salaryRange.Mode)
			};
		}

		private DataAccess.Model.Mode Map(HeadHunterApiClient.Dto.Mode mode)
		{
			return new DataAccess.Model.Mode
			{
				Name = mode.Name,
				Id = mode.Id
			};
		}

		private DataAccess.Model.Frequency Map(HeadHunterApiClient.Dto.Frequency frequency)
		{
			return new DataAccess.Model.Frequency
			{
				Id = frequency.Id,
				Name = frequency.Name
			};
		}

		private DbSearchOptions Map(VacancySearchOptions searchOptions)
		{
			return new DbSearchOptions
			{
				Employment = searchOptions.Employment,
				Experience = searchOptions.Experience,
				Salary = searchOptions.Salary,
				SalaryRangeFrequency = searchOptions.SalaryRangeFrequency,
				Schedule = searchOptions.Schedule
			};
		}

		private Dictionary<string, string> CreateHeadHunterSearchOptions(VacancySearchOptions searchOptions)
		{
			Dictionary<string, string> dict = new Dictionary<string, string>();
			
			foreach(var attr in typeof(VacancySearchOptions).GetProperties())
			{
				if(attr.GetValue(searchOptions) is string option && !string.IsNullOrWhiteSpace(option))
				{
					switch(attr.Name)
					{
						case "Experience":
							dict.Add("experience", option);
							break;
						case "Employment":
							dict.Add("employment", option);
							break;
						case "Schedule":
							dict.Add("schedule", option);
							break;
						case "EducationLevel":
							dict.Add("education_level", option);
							break;
						case "ProfessionalRole":
							dict.Add("professional_role", option);
							break;
						case "SalaryRangeFrequency":
							dict.Add("salary_range_mode", option);
							break;
						case "Salary":
							dict.Add("salary", option);
							break;

						default:
							continue;
					}
				}
			}
			return dict;
		}

		public async Task<List<VacancyResponse>> GetAllAsync()
		{
			List<Vacancy> vacancies = await _vacancyRepository.GetAllAsync();
			return vacancies.Select(x=> new VacancyResponse
			{
				Company = x.Company,
				CreatedVacancyDate = x.CreatedVacancyDate,
				Requirement = x.Requirement,
				Responsibility = x.Responsibility,
				Employment = x.Employment,
				Link = x.Link,
				Name = x.Name,
				Salary = Map(x.Salary),
				Schedule = x.Schedule,
				WorkExperience = Map(x.WorkExperience),
				WorkFormat = x.WorkFormat,
				WorkingHours = x.WorkingHours,
				WorkScheduleByDays = x.WorkScheduleByDays,
				UniqName = x.UniqName
			}).ToList();
		}

		private Dto.Experience Map(DataAccess.Model.Experience workExperience)
		{
			return new Dto.Experience
			{
				Name = workExperience.Name,
				Id = workExperience.Id
			};
		}

		private Dto.SalaryRange Map(DataAccess.Model.SalaryRange? salary)
		{
			return new Dto.SalaryRange
			{
				Currency = salary?.Currency,
				Frequency = Map(salary?.Frequency),
				From = salary?.From,
				Gross = salary?.Gross,
				Mode = Map(salary?.Mode),
				To = salary?.To
			};
		}

		private Dto.Mode Map(DataAccess.Model.Mode? mode)
		{
			return new Dto.Mode
			{
				Id = mode?.Id,
				Name = mode?.Name
			};
		}

		private Dto.Frequency Map(DataAccess.Model.Frequency? frequency)
		{
			return new Dto.Frequency
			{
				Id = frequency?.Id,
				Name = frequency?.Name
			};
		}

		public async Task<VacancyResponse> GetByIdAsync(Guid id)
		{
			Vacancy vacancy = await _vacancyRepository.GetByIdAsync(id);
			return new VacancyResponse
			{
				Company = vacancy.Company,
				CreatedVacancyDate = vacancy.CreatedVacancyDate,
				Requirement = vacancy.Requirement,
				Responsibility = vacancy.Responsibility,
				Employment = vacancy.Employment,
				Link = vacancy.Link,
				Name = vacancy.Name,
				Salary = Map(vacancy.Salary),
				Schedule = vacancy.Schedule,
				WorkExperience = Map(vacancy.WorkExperience),
				WorkFormat = vacancy.WorkFormat,
				WorkingHours = vacancy.WorkingHours,
				WorkScheduleByDays = vacancy.WorkScheduleByDays,
				UniqName = vacancy.UniqName
			};
		}
	}
}

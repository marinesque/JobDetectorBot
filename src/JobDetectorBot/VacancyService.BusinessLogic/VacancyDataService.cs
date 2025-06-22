using MongoDB.Driver;
using System.Data;
using System.Net.Http.Headers;
using System.Xml.Linq;
using VacancyService.DataAccess.Model;
using VacancyService.DataAccess.Repository;
using VacancyService.Dto;
using VacancyService.HeadHunterApiClient;
using VacancyService.HeadHunterApiClient.Dto;

namespace VacancyService.BusinessLogic
{
	public class VacancyDataService : IVacancyDataService
	{

		private readonly IVacancyRepository _vacancyRepository;
		private readonly IServiceClient _hhClient;

		public VacancyDataService(IVacancyRepository vacancyRepository,
			IServiceClient hhClient)
		{
			_vacancyRepository = vacancyRepository;
			_hhClient = hhClient;
		}

		public async Task DeleteAsync(string id)
		{
			await _vacancyRepository.DeleteAsync(id);
		}

		public async Task<List<VacancyResponse>> FindVacancy(string search, VacancySearchOptions searchOptions)
		{
			DbSearchOptions dbSearchOptions = Map(searchOptions);

			Dictionary<string, string> hhSearchOptions = CreateHeadHunterSearchOptions(searchOptions);

			Root hhResult;
			List<Vacancy> vacanciesInDb;

			var vacancyPositions = await _hhClient.GetVacancyPositions(search);

			if (vacancyPositions?.Items?.Any() == true)
			{
				List<string> vacancyNames = vacancyPositions?.Items?.Select(s => s.Text).ToList();

				vacanciesInDb = await _vacancyRepository.GetAllBySearchString(vacancyNames, dbSearchOptions);
			}
			else
			{
				vacanciesInDb = await _vacancyRepository.GetAllBySearchString(search, dbSearchOptions);
			}

			if(vacanciesInDb.Any())
			{
				return vacanciesInDb.Select(x => new VacancyResponse
				{
					Address = x.Address?.Raw,
					Archived = x.Archived,
					Area = x.Area?.Name,
					CreatedVacancyDate = x.CreatedVacancyDate,
					Employer = x.Employer?.Name,
					Employment = MapEmploymentDto(x.Employment),
					EmploymentForm = MapEmploymentFormDto(x.EmploymentForm),
					ExternalId = x.ExternalId,
					Link = x.Link,
					Name = x.Name,
					PublishedVacancyDate = x.PublishedVacancyDate,
					Requirement = x.Requirement,
					Responsibility = x.Responsibility,
					Salary = MapSalaryDto(x.Salary),
					Schedule = x.Schedule?.Name,
					Type = x.Type?.Name,
					UniqName = x.UniqName,
					WorkExperience = MapWorkExperienceDto(x.WorkExperience),
					WorkFormat = x.WorkFormat?.Select(z => z.Name).ToList(),
					WorkingHours = x.WorkingHours?.Select(z => z.Name).ToList(),
					WorkScheduleByDays = x.WorkScheduleByDays?.Select(z => z.Name).ToList()
				}).ToList();
			}
			else
			{

				hhResult = await _hhClient.GetVacancies(search, hhSearchOptions);

				if (hhResult != null)
				{
					await _vacancyRepository.BulkInsertAsync(hhResult.Items.Select(x => new Vacancy
					{
						Employer = MapEmployer(x.Employer),
						CreatedVacancyDate = x.CreatedAt,
						PublishedVacancyDate = x.PublishedAt,
						Requirement = x.Snippet.Requirement,
						Responsibility = x.Snippet.Responsibility,
						Employment = MapEmployment(x.Employment),
						Link = x.AlternateUrl,
						Name = x.Name,
						Salary = Map(x.SalaryRange),
						Schedule = MapSchedule(x.Schedule),
						WorkExperience = Map(x.Experience),
						WorkFormat = x.WorkFormat.Select(z => MapWorkFormat(z)).ToList(),
						WorkingHours = x.WorkingHours.Select(z => MapWorkingHour(z)).ToList(),
						WorkScheduleByDays = x.WorkScheduleByDays.Select(z => MapWorkScheduleByDay(z)).ToList(),
						UniqName = $"hh_{x.Id}",
						Create = DateTime.Now,
						EmploymentForm = MapEmploymentForm(x.EmploymentForm),
						Archived = x.Archived,
						Area = MapArea(x.Area),
						Address = MapAddress(x.Address),
						Type = MapType(x.Type),
						ExternalId = x.Id
					}).ToList());
				}

				return hhResult?.Items?.Select(x => new VacancyResponse
				{
					Address = x.Address?.Raw,
					Archived = x.Archived,
					Area = x.Area?.Name,
					CreatedVacancyDate = x.CreatedAt,
					Employer = x.Employer?.Name,
					Employment = MapEmploymentDto(x.Employment),
					EmploymentForm = MapEmploymentFormDto(x.EmploymentForm),
					ExternalId = x.Id,
					Link = x.AlternateUrl,
					Name = x.Name,
					PublishedVacancyDate = x.PublishedAt,
					Requirement = x.Snippet?.Requirement,
					Responsibility = x.Snippet?.Responsibility,
					Salary = MapSalaryRange(x.SalaryRange),
					Schedule = x.Schedule?.Name,
					Type = x.Type?.Name,
					UniqName = $"hh_{x.Id}",
					WorkExperience = MapWorkExperience(x.Experience),
					WorkFormat = x.WorkFormat?.Select(z => z.Name).ToList(),
					WorkingHours = x.WorkingHours?.Select(z => z.Name).ToList(),
					WorkScheduleByDays = x.WorkScheduleByDays?.Select(z => z.Name).ToList()
				}).ToList();
			}



			//if (vacanciesInDb.Any())
			//{

			//	hhResult = await _hhClient.GetVacancies(search, hhSearchOptions);

			//	if(vacanciesInDb.Count < hhResult.Items.Count)
			//	{
			//		TryAddNewItems(vacanciesInDb, hhResult.Items);
			//		vacanciesInDb = await _vacancyRepository.GetAllBySearchString(search, dbSearchOptions);
			//	}

			//	return vacanciesInDb.Select(x => new VacancyResponse
			//	{
			//		Company = x.Company,
			//		CreatedVacancyDate = x.CreatedVacancyDate,
			//		Requirement = x.Requirement,
			//		Responsibility = x.Responsibility,
			//		Employment = x.Employment,
			//		Link = x.Link,
			//		Name = x.Name,
			//		Salary = Map(x.Salary),
			//		Schedule = x.Schedule,
			//		WorkExperience = Map(x.WorkExperience),
			//		WorkFormat = x.WorkFormat,
			//		WorkingHours = x.WorkingHours,
			//		WorkScheduleByDays = x.WorkScheduleByDays,
			//		UniqName = x.UniqName,
			//	}).ToList();
			//}
			//else
			//{

			//}



		}

		private Dto.SalaryRange MapSalaryRange(HeadHunterApiClient.Dto.SalaryRange salaryRange)
		{
			if (salaryRange == null) return null;

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

		private Dto.EmploymentForm MapEmploymentFormDto(HeadHunterApiClient.Dto.EmploymentForm employmentForm)
		{
			if (employmentForm == null) return null;

			return new Dto.EmploymentForm
			{
				Id = employmentForm.Id,
				Name = employmentForm.Name
			};
		}

		private Dto.Employment MapEmploymentDto(HeadHunterApiClient.Dto.Employment employment)
		{
			if (employment == null) return null;

			return new Dto.Employment
			{
				Id = employment.Id,
				Name = employment.Name
			};
		}

		private VacancyType MapType(HeadHunterApiClient.Dto.Type type)
		{
			if(type == null) return null;

			return new VacancyType
			{
				Id = type.Id,
				Name = type.Name
			};
		}

		private DataAccess.Model.Address MapAddress(HeadHunterApiClient.Dto.Address address)
		{
			if (address == null) return null;

			return new DataAccess.Model.Address
			{
				Id = address.Id,
				Building = address.Building,
				City = address.City,
				Description = address.Description,
				Lat = address.Lat,
				Lng = address.Lng,
				Metro = MapMetro(address.Metro),
				MetroStations = address.MetroStations.Select(x => MapMetroStation(x)).ToList(),
				Raw = address.Raw,
				Street = address.Street
			};
		}

		private DataAccess.Model.Metro MapMetro(HeadHunterApiClient.Dto.Metro metro)
		{
			if (metro == null) return null;
			return new DataAccess.Model.Metro
			{
				Lat = metro.Lat,
				LineId = metro.LineId,
				LineName = metro.LineName,
				Lng = metro.Lng,
				StationId = metro.StationId,
				StationName = metro.StationName
			};
		}

		private DataAccess.Model.MetroStation MapMetroStation(HeadHunterApiClient.Dto.MetroStation x)
		{
			if (x == null) return null;

			return new DataAccess.Model.MetroStation
			{
				Lat= x.Lat,
				LineId= x.LineId,
				LineName= x.LineName,
				Lng = x.Lng,
				StationId= x.StationId,
				StationName = x.StationName
			};
		}

		private DataAccess.Model.Area MapArea(HeadHunterApiClient.Dto.Area area)
		{
			if (area == null) return null;

			return new DataAccess.Model.Area
			{
				Id = area.Id,
				Name = area.Name,
				Url = area.Url
			};
		}

		private DataAccess.Model.EmploymentForm MapEmploymentForm(HeadHunterApiClient.Dto.EmploymentForm employmentForm)
		{
			if (employmentForm == null) return null;

			return new DataAccess.Model.EmploymentForm
			{
				Id = employmentForm.Id,
				Name = employmentForm.Name
			};
		}

		private DataAccess.Model.WorkScheduleByDay MapWorkScheduleByDay(HeadHunterApiClient.Dto.WorkScheduleByDay z)
		{
			if (z == null) return null;

			return new DataAccess.Model.WorkScheduleByDay
			{
				Id = z.Id,
				Name = z.Name
			};
		}

		private DataAccess.Model.WorkingHour MapWorkingHour(HeadHunterApiClient.Dto.WorkingHour z)
		{
			if (z == null) return null;

			return new DataAccess.Model.WorkingHour
			{
				Id = z.Id,
				Name = z.Name
			};
		}

		private DataAccess.Model.WorkFormat MapWorkFormat(HeadHunterApiClient.Dto.WorkFormat z)
		{
			if (z == null) return null;

			return new DataAccess.Model.WorkFormat
			{
				Id = z.Id,
				Name = z.Name
			};
		}

		private DataAccess.Model.Schedule MapSchedule(HeadHunterApiClient.Dto.Schedule schedule)
		{
			if(schedule == null) { return null; }

			return new DataAccess.Model.Schedule
			{
				Id = schedule.Id,
				Name = schedule.Name
			};
		}

		private DataAccess.Model.Employment MapEmployment(HeadHunterApiClient.Dto.Employment employment)
		{
			if (employment == null) { return null; }

			return new DataAccess.Model.Employment
			{
				Id = employment.Id,
				Name = employment.Name
			};
		}

		private DataAccess.Model.Employer MapEmployer(HeadHunterApiClient.Dto.Employer employer)
		{
			if (employer == null)
			{
				return null;
			}

			return new DataAccess.Model.Employer
			{
				Id = employer.Id,
				Name = employer.Name,
				Url = employer.Url
			};
		}

		private async Task TryAddNewItems(List<Vacancy> vacanciesInDb, List<Item> hhItems)
		{
			var items = hhItems.Select(x => x.Id).Except(vacanciesInDb.Select(s => s.ExternalId));
			var toAdd = hhItems.Where(x => items.Contains(x.Id));
			await _vacancyRepository.BulkInsertAsync(toAdd.Select(x => new Vacancy
			{
				Employer = MapEmployer(x.Employer),
				CreatedVacancyDate = x.PublishedAt,
				Requirement = x.Snippet.Requirement,
				Responsibility = x.Snippet.Responsibility,
				Employment = MapEmployment(x.Employment),
				Link = x.AlternateUrl,
				Name = x.Name,
				Salary = Map(x.SalaryRange),
				Schedule = MapSchedule(x.Schedule),
				WorkExperience = Map(x.Experience),
				WorkFormat = x.WorkFormat.Select(x => MapWorkFormat(x)).ToList(),
				WorkingHours = x.WorkingHours.Select(x => MapWorkingHour(x)).ToList(),
				WorkScheduleByDays = x.WorkScheduleByDays.Select(x => MapWorkScheduleByDay(x)).ToList(),
				UniqName = $"hh_{x.Id}",
				Create = DateTime.Now,
				EmploymentForm = MapEmploymentForm(x.EmploymentForm),
				ExternalId = x.Id
			}).ToList());
		}

		private Dto.Experience MapWorkExperience(HeadHunterApiClient.Dto.Experience experience)
		{
			if (experience == null)
				return null;

			return new Dto.Experience
			{
				Id = experience.Id,
				Name = experience.Name
			};
		}

		private Dto.SalaryRange MapSalary(HeadHunterApiClient.Dto.SalaryRange salaryRange)
		{
			if (salaryRange == null)
				return null;

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
			if (mode == null)
				return null;

			return new Dto.Mode
			{
				Name = mode.Name,
				Id = mode.Id,
			};
		}

		private Dto.Frequency MapFrequency(HeadHunterApiClient.Dto.Frequency frequency)
		{
			if (frequency == null)
				return null;

			return new Dto.Frequency
			{
				Id = frequency.Id,
				Name = frequency.Name
			};
		}

		private DataAccess.Model.Experience Map(HeadHunterApiClient.Dto.Experience experience)
		{
			if (experience == null)
				return null;

			return new DataAccess.Model.Experience
			{
				Id = experience.Id,
				Name = experience.Name
			};
		}

		private DataAccess.Model.SalaryRange Map(HeadHunterApiClient.Dto.SalaryRange salaryRange)
		{
			if (salaryRange == null)
				return null;

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
			if (mode == null)
				return null;

			return new DataAccess.Model.Mode
			{
				Name = mode.Name,
				Id = mode.Id
			};
		}

		private DataAccess.Model.Frequency Map(HeadHunterApiClient.Dto.Frequency frequency)
		{
			if(frequency == null)
			{
				return null;
			}

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
				Address = x.Address?.Raw,
				Archived = x.Archived,
				Area = x.Area?.Name,
				CreatedVacancyDate = x.CreatedVacancyDate,
				Employer = x.Employer?.Name,
				Employment = MapEmploymentDto(x.Employment),
				EmploymentForm = MapEmploymentFormDto(x.EmploymentForm),
				ExternalId = x.ExternalId,
				Link = x.Link,
				Name = x.Name,
				PublishedVacancyDate = x.PublishedVacancyDate,
				Requirement = x.Requirement,
				Responsibility = x.Responsibility,
				Salary = MapSalaryDto(x.Salary),
				Schedule = x.Schedule?.Name,
				Type = x.Type?.Name,
				UniqName = x.UniqName,
				WorkExperience = MapWorkExperienceDto(x.WorkExperience),
				WorkFormat = x.WorkFormat?.Select(z=>z.Name).ToList(),
				WorkingHours = x.WorkingHours?.Select(z=>z.Name).ToList(),
				WorkScheduleByDays = x.WorkScheduleByDays?.Select(z=>z.Name).ToList()
			}).ToList();
		}

		private Dto.Experience MapWorkExperienceDto(DataAccess.Model.Experience workExperience)
		{
			if (workExperience == null) return null;

			return new Dto.Experience
			{
				Id = workExperience.Id,
				Name = workExperience.Name
			};
		}

		private Dto.SalaryRange MapSalaryDto(DataAccess.Model.SalaryRange? salary)
		{
			if (salary == null) return null;

			return new Dto.SalaryRange
			{
				Currency = salary.Currency,
				Frequency = MapFrequencyDto(salary.Frequency),
				From = salary.From,
				Gross = salary.Gross,
				Mode = MapModeDto(salary.Mode),
				To = salary.To
			};
		}

		private Dto.Mode MapModeDto(DataAccess.Model.Mode mode)
		{
			if (mode == null) return null;

			return new Dto.Mode
			{
				Id = mode.Id,
				Name = mode.Name
			};
		}

		private Dto.Frequency MapFrequencyDto(DataAccess.Model.Frequency? frequency)
		{
			if (frequency == null) return null;

			return new Dto.Frequency
			{
				Id = frequency.Id,
				Name = frequency.Name
			};
		}

		private Dto.EmploymentForm MapEmploymentFormDto(DataAccess.Model.EmploymentForm? employmentForm)
		{
			if (employmentForm == null) return null;

			return new Dto.EmploymentForm
			{
				Id = employmentForm?.Id,
				Name = employmentForm?.Name
			};
		}

		private Dto.Employment MapEmploymentDto(DataAccess.Model.Employment? employment)
		{
			if (employment == null) { return null; }

			return new Dto.Employment
			{
				Id = employment?.Id,
				Name = employment?.Name
			};
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

		public async Task<VacancyResponse> GetByIdAsync(string id)
		{
			Vacancy vacancy = await _vacancyRepository.GetByIdAsync(id);
			return new VacancyResponse
			{
				Address = vacancy.Address?.Raw,
				Archived = vacancy.Archived,
				Area = vacancy.Area?.Name,
				CreatedVacancyDate = vacancy.CreatedVacancyDate,
				Employer = vacancy.Employer?.Name,
				Employment = MapEmploymentDto(vacancy.Employment),
				EmploymentForm = MapEmploymentFormDto(vacancy.EmploymentForm),
				ExternalId = vacancy.ExternalId,
				Link = vacancy.Link,
				Name = vacancy.Name,
				PublishedVacancyDate = vacancy.PublishedVacancyDate,
				Requirement = vacancy.Requirement,
				Responsibility = vacancy.Responsibility,
				Salary = MapSalaryDto(vacancy.Salary),
				Schedule = vacancy.Schedule?.Name,
				Type = vacancy.Type?.Name,
				UniqName = vacancy.UniqName,
				WorkExperience = MapWorkExperienceDto(vacancy.WorkExperience),
				WorkFormat = vacancy.WorkFormat?.Select(z=>z.Name).ToList(),
				WorkingHours = vacancy.WorkingHours?.Select(z=>z.Name).ToList(),
				WorkScheduleByDays = vacancy.WorkScheduleByDays?.Select(z=>z.Name).ToList()
			};
		}

	}
}

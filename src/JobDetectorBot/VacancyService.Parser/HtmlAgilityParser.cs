using VacancyService.Configuration;
using VacancyService.Parser.Dto;
using HtmlAgilityPack;
using System.Xml.Linq;



namespace VacancyService.Parser
{
	public class HtmlAgilityParser : IParser<SiteVacancy, SiteSearchParam>
	{

		private readonly string _url;

		public HtmlAgilityParser(IGrabberConfiguration grabberConfiguration)
		{
			_url = grabberConfiguration.GetHeadHunterConfiguration();
		}

		public async Task<List<SiteVacancy>> GetVacancies(SiteSearchParam searchParam)
		{
			List<SiteVacancy> localData = new List<SiteVacancy>();
			string searchParams = GetSearchParams(searchParam.SearchKeyWords);
			string url = CreateUrl(_url, searchParams, searchParam.SearchString);

			var web = new HtmlWeb();
			var doc = web.Load(url);

			var nodes = doc.DocumentNode.SelectNodes("//h2[@data-qa]");

			foreach (var node in nodes)
			{
				string link = node.SelectSingleNode("span/a").GetAttributeValue("href", "");
				 if(link.Contains("adsrv.hh.ru"))
				{
					continue;
				}
				var innerWeb = new HtmlWeb();
				var innerDocument = innerWeb.Load(link);

				var vacancyName = innerDocument.DocumentNode.SelectSingleNode("//*[contains(@data-qa,'vacancy-title')]")?.InnerText;
				var salary = innerDocument.DocumentNode.SelectSingleNode("//*[contains(@data-qa,'vacancy-salary-compensation-type-gross')]")?.InnerText ?? innerDocument.DocumentNode.SelectSingleNode("//*[contains(@data-qa,'vacancy-salary-compensation-type-net')]")?.InnerText;

				string exp = innerDocument.DocumentNode.SelectSingleNode("//*[contains(@data-qa,'vacancy-experience')]")?.InnerText;
				string workType = innerDocument.DocumentNode.SelectSingleNode("//*[contains(@data-qa,'common-employment-text')]")?.InnerText;
				string schedule = innerDocument.DocumentNode.SelectSingleNode("//*[contains(@data-qa,'work-schedule-by-days-text')]")?.InnerText.Split(':', StringSplitOptions.None).LastOrDefault();
				string work_time = innerDocument.DocumentNode.SelectSingleNode("//*[contains(@data-qa,'working-hours-text')]")?.InnerText.Split(':', StringSplitOptions.None).LastOrDefault();
				string work_format = innerDocument.DocumentNode.SelectSingleNode("//*[contains(@data-qa,'work-formats-text')]")?.InnerText.Split(':', StringSplitOptions.None).LastOrDefault();

				string description = innerDocument.DocumentNode.SelectSingleNode("//*[contains(@data-qa,'vacancy-description')]")?.InnerText;
				string createdVacancyDateFromSite = innerDocument.DocumentNode.SelectSingleNode("//*[contains(@class,'vacancy-creation-time-redesigned')]")?.InnerText;
				string createdVacancyDate = GetVacancyDate(createdVacancyDateFromSite);
				string company = innerDocument.DocumentNode.SelectSingleNode("//*[contains(@data-qa,'vacancy-company-name')]")?.InnerText;

				localData.Add(new SiteVacancy
				{
					Link = link,
					Name = vacancyName,
					Salary = salary,
					WorkTime = work_time,
					WorkFormat = work_format,
					WorkExperience = exp,
					Schedule = schedule,
					WorkType = workType,
					Description = description,
					CreatedVacancyDate = createdVacancyDate,
					Company = company
				});

			}

			return localData;
		}

		private static string GetVacancyDate(string createVacancyDateFromSite)
		{
			if(string.IsNullOrEmpty(createVacancyDateFromSite))
			{
				return string.Empty;
			}
			int indexOfPlace = createVacancyDateFromSite.Replace("Вакансия опубликована", "").IndexOf(" в "); ;
			string createVacancyDate = createVacancyDateFromSite.Replace("Вакансия опубликована", "").Substring(0, indexOfPlace);
			return createVacancyDate;
		}

		private string CreateUrl(string url, string path, string searchString)
		{
			return string.Concat(url, '?', path, "&text=", searchString);
		}

		private string GetSearchParams(SearchKeyWords searchKeyWords)
		{
			switch(searchKeyWords.ToString())
			{
				case "1":
					return "search_field=name";
				case "2":
					return "search_field=company_name";
				case "3":
					return "search_field=name&search_field=company_name";
				case "4":
					return "search_field=description";
				case "5":
					return "search_field=name&search_field=description";
				case "6":
					return "search_field=company_name&search_field=description";
				case "7":
					return "search_field=name&search_field=company_name&search_field=description";
				default:
					return "search_field=name&search_field=company_name&search_field=description";
			}			
		}
	}
}

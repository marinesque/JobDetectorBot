using HeadHunterGrabber.Parser.Dto;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace HeadHunterGrabber.Parser
{
	public class SeleniumParser : IParser<SiteVacancy, SiteSearchParam>
	{

		private readonly string _url;

		public SeleniumParser()
		{
			_url = "https://hh.ru/search/vacancy?text=";
		}

		public async Task<List<SiteVacancy>> GetVacancies(SiteSearchParam searchParam)
		{
			var chromeOptions = new ChromeOptions();
			chromeOptions.AddArguments("headless");
			var driver = new ChromeDriver();

			string url = string.Concat(_url, searchParam.SearchString);

			driver.Navigate().GoToUrl(url);

			var nodes = driver.FindElements(By.ClassName("bloko-header-section-2"));

			foreach (var item in nodes)
			{
				var aTag = item.FindElement(By.TagName("a"));
				string link = aTag.GetAttribute("href");
				aTag.Click();

				var a = "str";
			}

			return new List<SiteVacancy>();
		}
	}
}

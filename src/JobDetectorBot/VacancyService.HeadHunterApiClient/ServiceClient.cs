using System.Net.Http;
using VacancyService.HeadHunterApiClient.Dto;
using Newtonsoft.Json;
using System.Web;
using System.Text;
using Microsoft.Extensions.Primitives;
using System;

namespace VacancyService.HeadHunterApiClient
{
	public class ServiceClient : IServiceClient
	{
		HttpClient _httpClient;

		public ServiceClient(IHttpClientFactory httpClientFactory)
		{
			_httpClient = httpClientFactory?.CreateClient();
		}

		public async Task<Root> GetVacancies(string searchText, Dictionary<string, string> searchParams)
		{
			string baseUri = "https://api.hh.ru/vacancies?";

			if (string.IsNullOrEmpty(searchText))
			{
				return new Root();
			}

			string content = await SearchData(searchText, searchParams, baseUri);

			Root root = JsonConvert.DeserializeObject<Root>(content);

			if (root.Pages > 1)
			{
				List<Item> Items = new List<Item>();
				Items.AddRange(root.Items);
				Root newRoot = new Root
				{
					Found = root.Found,
					Arguments = root.Arguments,
					AlternateUrl = root.AlternateUrl,
					Clusters = root.Clusters,
					Fixes = root.Fixes,
					Pages = root.Pages,
					PerPage = root.PerPage,
					Suggests = root.Suggests
				};

				for(int page = 1; page < root.Pages; page++)
				{
					string contentN = await SearchData(searchText, searchParams, baseUri, page);
					Root rootN = JsonConvert.DeserializeObject<Root>(contentN);
					Items.AddRange(rootN.Items);
				}
				newRoot.Items = Items;

				return newRoot;
			}

			return root;
		}

		private async Task<string> SearchData(string searchText, Dictionary<string, string> searchParams, string baseUri, int page = 0)
		{
			string baseQuery = $"&per_page=100&search_field=name&page={page}";

			string searchStringParams = string.Concat("text=", Uri.EscapeDataString(searchText), baseQuery);

			var uriBuilder = new UriBuilder(baseUri)
			{
				Query = string.Concat(searchStringParams, '&', string.Join("&", searchParams.Select(kvp => $"{kvp.Key}={kvp.Value}")))
			};

			_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PostmanRuntime/7.43.3");
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri.AbsoluteUri);
			HttpResponseMessage response = await _httpClient.SendAsync(request);
			return await response.Content.ReadAsStringAsync();
		}
	}
}

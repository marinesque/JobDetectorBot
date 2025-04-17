using Microsoft.Extensions.Options;


namespace VacancyService.Configuration
{
	public class GrabberConfiguration : IGrabberConfiguration
	{
		private readonly IOptions<HeadHunterSettings> _options;

		public GrabberConfiguration(IOptions<HeadHunterSettings> options)
		{
			_options = options;
		}

		public string GetHeadHunterConfiguration()
		{
			HeadHunterSettings headHunterSettings = _options.Value;
			return headHunterSettings.SiteString;
		}
	}
}


namespace VacancyService.Dto
{ 
    public class SalaryRange
    {
        public int? From { get; set; }

        public int? To { get; set; }

		public string Currency { get; set; }

		public bool? Gross { get; set; }

		public Mode Mode { get; set; }

		public Frequency Frequency { get; set; }
	}

}
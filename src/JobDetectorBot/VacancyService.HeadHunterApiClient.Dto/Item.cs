using Newtonsoft.Json; 
using System.Collections.Generic; 
using System; 
namespace VacancyService.HeadHunterApiClient.Dto{ 

    public class Item
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;

        [JsonProperty("premium", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Premium;

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name;

        [JsonProperty("department", NullValueHandling = NullValueHandling.Ignore)]
        public object Department;

        [JsonProperty("has_test", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasTest;

        [JsonProperty("response_letter_required", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ResponseLetterRequired;

        [JsonProperty("area", NullValueHandling = NullValueHandling.Ignore)]
        public Area Area;

        [JsonProperty("salary", NullValueHandling = NullValueHandling.Ignore)]
        public Salary Salary;

        [JsonProperty("salary_range", NullValueHandling = NullValueHandling.Ignore)]
        public SalaryRange SalaryRange;

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public Type Type;

        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public Address Address;

        [JsonProperty("response_url", NullValueHandling = NullValueHandling.Ignore)]
        public object ResponseUrl;

        [JsonProperty("sort_point_distance", NullValueHandling = NullValueHandling.Ignore)]
        public object SortPointDistance;

        [JsonProperty("published_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? PublishedAt;

        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CreatedAt;

        [JsonProperty("archived", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Archived;

        [JsonProperty("apply_alternate_url", NullValueHandling = NullValueHandling.Ignore)]
        public string ApplyAlternateUrl;

        [JsonProperty("show_logo_in_search", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowLogoInSearch;

        [JsonProperty("insider_interview", NullValueHandling = NullValueHandling.Ignore)]
        public object InsiderInterview;

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url;

        [JsonProperty("alternate_url", NullValueHandling = NullValueHandling.Ignore)]
        public string AlternateUrl;

        [JsonProperty("relations", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> Relations;

        [JsonProperty("employer", NullValueHandling = NullValueHandling.Ignore)]
        public Employer Employer;

        [JsonProperty("snippet", NullValueHandling = NullValueHandling.Ignore)]
        public Snippet Snippet;

        [JsonProperty("show_contacts", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ShowContacts;

        [JsonProperty("contacts", NullValueHandling = NullValueHandling.Ignore)]
        public object Contacts;

        [JsonProperty("schedule", NullValueHandling = NullValueHandling.Ignore)]
        public Schedule Schedule;

        [JsonProperty("working_days", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> WorkingDays;

        [JsonProperty("working_time_intervals", NullValueHandling = NullValueHandling.Ignore)]
        public List<WorkingTimeInterval> WorkingTimeIntervals;

        [JsonProperty("working_time_modes", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> WorkingTimeModes;

        [JsonProperty("accept_temporary", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AcceptTemporary;

        [JsonProperty("fly_in_fly_out_duration", NullValueHandling = NullValueHandling.Ignore)]
        public List<object> FlyInFlyOutDuration;

        [JsonProperty("work_format", NullValueHandling = NullValueHandling.Ignore)]
        public List<WorkFormat> WorkFormat;

        [JsonProperty("working_hours", NullValueHandling = NullValueHandling.Ignore)]
        public List<WorkingHour> WorkingHours;

        [JsonProperty("work_schedule_by_days", NullValueHandling = NullValueHandling.Ignore)]
        public List<WorkScheduleByDay> WorkScheduleByDays;

        [JsonProperty("night_shifts", NullValueHandling = NullValueHandling.Ignore)]
        public bool? NightShifts;

        [JsonProperty("professional_roles", NullValueHandling = NullValueHandling.Ignore)]
        public List<ProfessionalRole> ProfessionalRoles;

        [JsonProperty("accept_incomplete_resumes", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AcceptIncompleteResumes;

        [JsonProperty("experience", NullValueHandling = NullValueHandling.Ignore)]
        public Experience Experience;

        [JsonProperty("employment", NullValueHandling = NullValueHandling.Ignore)]
        public Employment Employment;

        [JsonProperty("employment_form", NullValueHandling = NullValueHandling.Ignore)]
        public EmploymentForm EmploymentForm;

        [JsonProperty("internship", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Internship;

        [JsonProperty("adv_response_url", NullValueHandling = NullValueHandling.Ignore)]
        public object AdvResponseUrl;

        [JsonProperty("is_adv_vacancy", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsAdvVacancy;

        [JsonProperty("adv_context", NullValueHandling = NullValueHandling.Ignore)]
        public object AdvContext;

        [JsonProperty("branding", NullValueHandling = NullValueHandling.Ignore)]
        public Branding Branding;
    }

}
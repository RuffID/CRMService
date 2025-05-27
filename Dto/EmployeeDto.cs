using Newtonsoft.Json;

namespace CRMService.Dto
{
    public class EmployeeDto
    {
        public int Id { get; set; }

        [JsonProperty("last_name")]        
        public string? LastName { get; set; }

        [JsonProperty("first_name")]
        public string? FirstName { get; set; }

        public string? Patronymic { get; set; }
    }
}

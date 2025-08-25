using Newtonsoft.Json;

namespace CRMService.Models.Dto.Entity
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

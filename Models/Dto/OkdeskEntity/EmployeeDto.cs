using Newtonsoft.Json;

namespace CRMService.Models.Dto.OkdeskEntity
{
    public class EmployeeDto
    {
        public int Id { get; set; }

        [JsonProperty("last_name")]        
        public string? LastName { get; set; }

        [JsonProperty("first_name")]
        public string? FirstName { get; set; }

        public string? Patronymic { get; set; }

        /// <summary>
        /// User's membership in groups.
        /// </summary>
        public List<int> GroupIds { get; set; } = new();
    }
}

using System.Text.Json.Serialization;

namespace CRMService.Contracts.Models.Dto.OkdeskEntity
{
    public class EmployeeDto
    {
        public int Id { get; set; }

        [JsonPropertyName("last_name")]        
        public string? LastName { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        public string? Patronymic { get; set; }

        public bool Active { get; set; }

        /// <summary>
        /// User's membership in groups.
        /// </summary>
        public List<GroupDto> Groups { get; set; } = new();
    }
}




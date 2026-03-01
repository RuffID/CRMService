using EFCoreLibrary.Abstractions.Entity;
using CRMService.Domain.Models.CrmEntities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CRMService.Domain.Models.OkdeskEntity
{
    public class Employee : IEntity<int>, ICopyable<Employee>
    {
        public int Id { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        public string? Patronymic { get; set; }

        public string? Position { get; set; }

        public bool Active { get; set; }

        public string? Email { get; set; }

        public string Login { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [NotMapped]
        public ICollection<OkdeskRole> Roles { get; set; } = new List<OkdeskRole>();

        public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();

        public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

        public virtual ICollection<EmployeeGroup> EmployeeGroups { get; set; } = new List<EmployeeGroup>();

        public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();

        public virtual ICollection<PlanSetting> PlanSettings { get; set; } = new List<PlanSetting>();

        public void CopyData(Employee employee)
        {
            FirstName = employee.FirstName;
            LastName = employee.LastName;
            Patronymic = employee.Patronymic;
            Position = employee.Position;
            Active = employee.Active;
            Email = employee.Email;
            Login = employee.Login;
            Phone = employee.Phone;
        }
    }
}



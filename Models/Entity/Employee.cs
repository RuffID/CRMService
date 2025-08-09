using CRMService.Interfaces.Entity;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMService.Models.Entity
{
    public class Employee : IEntity
    {
        public int Id { get; set; }

        [JsonProperty("first_name")]
        public string? FirstName { get; set; }

        [JsonProperty("last_name")]
        public string? LastName { get; set; }

        public string? Patronymic { get; set; }

        public string? Position { get; set; }

        public bool? Active { get; set; }

        public string? Email { get; set; }

        public string? Login { get; set; }

        public string? Phone { get; set; }

        [NotMapped]
        public ICollection<Role>? Roles { get; set; } = new List<Role>();

        public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();

        public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();

        public virtual ICollection<EmployeeGroup> EmployeeGroups { get; set; } = new List<EmployeeGroup>();

        public Employee() { }

        public Employee(int id)
        {
            Id = id;
        }

        public void CopyData(Employee newItem)
        {
            FirstName = newItem.FirstName;
            LastName = newItem.LastName;
            Patronymic = newItem.Patronymic;
            Position = newItem.Position;
            Active = newItem.Active;
            Email = newItem.Email;
            Login = newItem.Login;
            Phone = newItem.Phone;
        }
    }
}
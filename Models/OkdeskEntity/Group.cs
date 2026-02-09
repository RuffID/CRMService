using CRMService.Abstractions.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMService.Models.OkdeskEntity
{
    public class Group : IEntity<int>, ICopyable<Group>
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool Active { get; set; }

        public string? Description { get; set; }

        [NotMapped]
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();

        public virtual ICollection<EmployeeGroup> EmployeeGroups { get; set; } = new List<EmployeeGroup>();

        public void CopyData(Group newItem)
        {
            Name = newItem.Name;
            Active = newItem.Active;
            Description = newItem.Description;
        }
    }
}
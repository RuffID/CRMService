using CRMService.Dto;
using CRMService.Interfaces.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMService.Models.Entity
{
    public class Group : IEntity
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public bool? Active { get; set; }

        public string? Description { get; set; }

        [NotMapped]
        public IEnumerable<EmployeeDto>? Employees { get; set; } = new List<EmployeeDto>();

        public void CopyData(Group newItem)
        {
            Name = newItem.Name;
            Active = newItem.Active;
            Description = newItem.Description;
        }
    }
}
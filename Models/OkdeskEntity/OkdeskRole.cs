using CRMService.Abstractions.Entity;

namespace CRMService.Models.OkdeskEntity
{
    public class OkdeskRole : IEntity<int>, ICopyable<OkdeskRole>
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();

        public void CopyData(OkdeskRole newItem)
        {
            Name = newItem.Name;
        }
    }
}
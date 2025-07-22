using CRMService.Interfaces.Entity;

namespace CRMService.Models.Entity
{
    public class EmployeeGroup : IEntity
    {
        public int Id { get; set; }

        public int? EmployeeId { get; set; }

        public int? GroupId { get; set; }

        internal void CopyData(EmployeeGroup newItem)
        {
            EmployeeId = newItem.EmployeeId;
            GroupId = newItem.GroupId;
        }
    }
}
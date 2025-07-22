using CRMService.Interfaces.Entity;

namespace CRMService.Models.Entity
{
    public class EmployeeRole : IEntity
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public int RoleId { get; set; }

        public void CopyData(EmployeeRole newItem)
        {
            EmployeeId = newItem.EmployeeId;
            RoleId = newItem.RoleId;
        }
    }
}
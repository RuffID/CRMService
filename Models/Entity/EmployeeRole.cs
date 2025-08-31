using CRMService.Interfaces.Entity;

namespace CRMService.Models.Entity
{
    public class EmployeeRole : ICopyable<EmployeeRole>
    {
        public int EmployeeId { get; set; }

        public int RoleId { get; set; }

        public virtual Employee Employee { get; set; } = null!;

        public virtual OkdeskRole Role { get; set; } = null!;

        public void CopyData(EmployeeRole newItem)
        {
            EmployeeId = newItem.EmployeeId;
            RoleId = newItem.RoleId;
        }
    }
}
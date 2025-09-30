using CRMService.Interfaces.Entity;

namespace CRMService.Models.OkdeskEntity
{
    public class EmployeeGroup : ICopyable<EmployeeGroup>
    {
        public int EmployeeId { get; set; }

        public int GroupId { get; set; }

        public virtual Employee Employee { get; set; } = null!;

        public virtual Group Group { get; set; } = null!;

        public void CopyData(EmployeeGroup newItem)
        {
            EmployeeId = newItem.EmployeeId;
            GroupId = newItem.GroupId;
        }
    }
}
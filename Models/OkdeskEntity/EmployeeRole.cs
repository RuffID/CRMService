using CRMService.Abstractions.Entity;
using CRMService.Core;

namespace CRMService.Models.OkdeskEntity
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

        public static IEqualityComparer<EmployeeRole> Comparer { get; } = CompositeKeyComparer.For<EmployeeRole, int, int>(x => x.EmployeeId, x => x.RoleId);
    }
}
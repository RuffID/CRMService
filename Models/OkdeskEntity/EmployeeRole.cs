using CRMService.Interfaces.Entity;

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

        public static IEqualityComparer<EmployeeRole> Comparer { get; } = new KeyComparer();

        internal sealed class KeyComparer : IEqualityComparer<EmployeeRole>
        {
            public bool Equals(EmployeeRole? x, EmployeeRole? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return x.EmployeeId == y.EmployeeId && x.RoleId == y.RoleId;
            }

            public int GetHashCode(EmployeeRole obj)
                => HashCode.Combine(obj.EmployeeId, obj.RoleId);
        }
    }
}
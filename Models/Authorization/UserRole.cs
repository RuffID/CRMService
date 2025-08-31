namespace CRMService.Models.Authorization
{
    public class UserRole
    {
        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }

        public virtual CrmRole Role { get; set; } = null!;

        public virtual User User { get; set; } = null!;

        public void CopyData(UserRole newItem)
        {
            UserId = newItem.UserId;
            RoleId = newItem.RoleId;
        }
    }
}
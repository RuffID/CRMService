using CRMService.Interfaces.Entity;

namespace CRMService.Models.Dto.Authorization
{
    public class RoleDto : IEntity<Guid>
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
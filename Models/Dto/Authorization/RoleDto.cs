using CRMService.Interfaces.Authorization;

namespace CRMService.Models.Dto.Authorization
{
    public class RoleDto : IEntity
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }
    }
}
namespace CRMService.Contracts.Models.Request
{
    public class UpdateUserRequest
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public List<Guid>? RoleIds { get; set; }
    }
}




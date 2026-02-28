namespace CRMService.Models.Request
{
    public class CreateUserRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public List<Guid>? RoleIds { get; set; }
    }
}

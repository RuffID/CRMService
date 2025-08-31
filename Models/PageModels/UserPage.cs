using CRMService.Models.Enum;

namespace CRMService.Models.PageModels
{
    public class UserPage
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}

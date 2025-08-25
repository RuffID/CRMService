using CRMService.Models.Enum;

namespace CRMService.Models.PageModels
{
    public class UserPage
    {
        public Guid Id { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public string? Name { get; set; }
        public UserRole Role { get; set; }
    }
}

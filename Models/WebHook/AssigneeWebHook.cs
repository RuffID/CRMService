using CRMService.Models.Entity;

namespace CRMService.Models.WebHook
{
    public class AssigneeWebHook
    {
        public Group? Group { get; set; }
        public Employee? Employee { get; set; }
    }
}

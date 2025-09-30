using CRMService.Models.OkdeskEntity;

namespace CRMService.Models.WebHook
{
    public class AssigneeWebHook
    {
        public Group? Group { get; set; }
        public Employee? Employee { get; set; }
    }
}

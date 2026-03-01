using CRMService.Domain.Models.OkdeskEntity;

namespace CRMService.Application.Models.WebHook
{
    public class AssigneeWebHook
    {
        public Group? Group { get; set; }
        public Employee? Employee { get; set; }
    }
}




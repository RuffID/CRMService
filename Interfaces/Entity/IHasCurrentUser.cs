using CRMService.Models.Authorization;

namespace CRMService.Interfaces.Entity
{
    public interface IHasCurrentUser
    {
        User CurrentUser { get; set; }
    }
}

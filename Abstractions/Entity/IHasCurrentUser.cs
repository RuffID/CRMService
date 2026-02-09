using CRMService.Models.Authorization;

namespace CRMService.Abstractions.Entity
{
    public interface IHasCurrentUser
    {
        User CurrentUser { get; set; }
    }
}

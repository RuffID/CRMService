using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Abstractions.Entity
{
    public interface IHasCurrentUser
    {
        User CurrentUser { get; set; }
    }
}




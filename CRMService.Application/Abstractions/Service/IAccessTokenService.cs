using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Abstractions.Service
{
    public interface IAccessTokenService
    {
        string Create(User user);
    }
}
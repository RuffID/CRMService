using CRMService.Domain.Models.Authorization;

namespace CRMService.Application.Abstractions.Service
{
    public interface IAccessTokenService
    {
        string Create(User user);
    }

    public interface IRandomStringGenerator
    {
        string GetRandomString(int length = 12);
        string GetBase64RandomString();
    }
}




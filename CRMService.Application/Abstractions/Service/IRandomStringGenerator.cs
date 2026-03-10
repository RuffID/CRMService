namespace CRMService.Application.Abstractions.Service
{
    public interface IRandomStringGenerator
    {
        string GetRandomString(int length = 12);
        string GetBase64RandomString();
    }
}
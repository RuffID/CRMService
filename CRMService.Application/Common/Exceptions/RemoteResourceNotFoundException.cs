namespace CRMService.Application.Common.Exceptions
{
    public class RemoteResourceNotFoundException(string resource, Exception? innerException = null)
        : ExternalServiceException($"Remote resource not found: {resource}", innerException);
}
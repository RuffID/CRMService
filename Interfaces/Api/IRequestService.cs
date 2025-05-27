using System.Runtime.CompilerServices;

namespace CRMService.Interfaces.Api
{
    public interface IRequestService
    {
        Task<string?> SendPost(string link, StringContent content, [CallerMemberName] string caller = "");
        Task<string?> SendGet(string link, [CallerMemberName] string caller = "");
    }
}

using CRMService.Models.Request;
using System.Runtime.CompilerServices;

namespace CRMService.Interfaces.Api
{
    public interface IRequestService
    {
        Task<string?> SendPost(string link, StringContent content, [CallerMemberName] string caller = "");
        Task<ResponseStream> SendGet(string link, CancellationToken ct, [CallerMemberName] string caller = "");
    }
}

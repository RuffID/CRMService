using CRMService.Interfaces.Api;
using System.Runtime.CompilerServices;

namespace CRMService.API
{
    public class RequestClient(HttpClient httpClient, ILoggerFactory logger) : IRequestService
    {
        private readonly ILogger<RequestClient> _logger = logger.CreateLogger<RequestClient>();

        public async Task<string?> SendPost(string link, StringContent content, [CallerMemberName] string caller = "")
        {
            try
            {
                using var response = await httpClient.PostAsync(link, content);
                string responseString = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("[Method:{MethodName}] Wrong api key. Error: {Error}. Link: {Link}. Caller: {CallerMethod}", nameof(SendPost), responseString, link, caller);
                    return null;
                }
                if (response.IsSuccessStatusCode == false)
                {
                    _logger.LogWarning("[Method:{MethodName}] Not success response code. Link: {Link}, response code: {responseCode}. Caller: {CallerMethod}", nameof(SendPost), link, response.StatusCode, caller);
                    return null;
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error while send post async request. Caller: {CallerMethod}", nameof(SendPost), caller);
                return null;
            }
        }

        public async Task<string?> SendGet(string link, [CallerMemberName] string caller = "")
        {
            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(link);
                string responseString = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("[Method:{MethodName}] Wrong api key. Error: {Error}. Link: {Link}, response code: {responseCode}. Caller: {CallerMethod}", nameof(SendGet), responseString, link, response.StatusCode, caller);
                    return null;
                }
                if (response.IsSuccessStatusCode == false)
                {
                    _logger.LogWarning("[Method:{MethodName}] Not success response code. Link: {Link}. Caller: {CallerMethod}", nameof(SendGet), link, caller);
                    return null;
                }

                return responseString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error while send get async request. Caller: {CallerMethod}", nameof(SendGet), caller);
                return null;
            }
        }
    }
}

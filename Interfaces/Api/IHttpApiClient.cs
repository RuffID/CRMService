namespace CRMService.Interfaces.Api
{
    public interface IHttpApiClient
    {
        Task<T?> SendAsync<T>(API.HttpRequestOptions options, CancellationToken ct = default);
        Task<T?> GetAsync<T>(string url, IDictionary<string, string>? headers = null, CancellationToken ct = default);
        Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body, string? contentType = null, IDictionary<string, string>? headers = null, CancellationToken ct = default);
        Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest body, string? contentType = null, IDictionary<string, string>? headers = null, CancellationToken ct = default);
        Task<TResponse?> PatchAsync<TRequest, TResponse>(string url, TRequest body, string? contentType = null, IDictionary<string, string>? headers = null, CancellationToken ct = default);
        Task<TResponse?> DeleteAsync<TResponse>(string url, IDictionary<string, string>? headers = null, CancellationToken ct = default);
        Task<TResponse?> DeleteAsync<TRequest, TResponse>(string url, TRequest body, string? contentType = null, IDictionary<string, string>? headers = null, CancellationToken ct = default);
        Task PostAsync(string url, object body, string? contentType = null, IDictionary<string, string>? headers = null, CancellationToken ct = default);
        Task DeleteAsync(string url, IDictionary<string, string>? headers = null, CancellationToken ct = default);
        Task PatchAsync(string url, object body, string? contentType = null, IDictionary<string, string>? headers = null, CancellationToken ct = default);
        Task PutAsync(string url, object body, string? contentType = null, IDictionary<string, string>? headers = null, CancellationToken ct = default);
    }
}
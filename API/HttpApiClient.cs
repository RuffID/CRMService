using Azure;
using CRMService.Interfaces.Api;
using CRMService.Models.Exceptions;
using System.Net.Http.Headers;
using System.Text;

namespace CRMService.API
{
    public class HttpApiClient(HttpClient http, IJsonSerializer serializer, ILoggerFactory? logger = null) : IHttpApiClient
    {
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);
        private const int ERROR_SNIPPET_MAX_CHARS = 2000;
        private static readonly HttpMethod Patch = new ("PATCH");
        private readonly HttpClient _http = http ?? throw new ArgumentNullException(nameof(http));
        private readonly IJsonSerializer _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        private readonly ILogger<HttpApiClient>? _logger = logger?.CreateLogger<HttpApiClient>();

        public Task<T?> GetAsync<T>(string url, IDictionary<string, string>? headers = null, CancellationToken ct = default)
            => SendAsync<T>(new HttpRequestOptions { Method = HttpMethod.Get, Url = url, Headers = headers }, ct);

        public Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body, string? contentType = null, IDictionary<string, string>? headers = null, CancellationToken ct = default)
            => SendAsync<TResponse>(new HttpRequestOptions { Method = HttpMethod.Post, Url = url, Body = body, BodyContentType = contentType, Headers = headers }, ct);

        public Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest body, string? contentType = null, IDictionary<string, string>? headers = null, CancellationToken ct = default)
            => SendAsync<TResponse>(new HttpRequestOptions { Method = HttpMethod.Put, Url = url, Body = body, BodyContentType = contentType, Headers = headers }, ct);

        public Task<TResponse?> PatchAsync<TRequest, TResponse>(string url, TRequest body, string? contentType = null, IDictionary<string, string>? headers = null, CancellationToken ct = default)
            => SendAsync<TResponse>(new HttpRequestOptions { Method = Patch, Url = url, Body = body, BodyContentType = contentType, Headers = headers }, ct);

        public Task<TResponse?> DeleteAsync<TResponse>(string url, IDictionary<string, string>? headers = null, CancellationToken ct = default)
            => SendAsync<TResponse>(new HttpRequestOptions { Method = HttpMethod.Delete, Url = url, Headers = headers }, ct);

        public Task<TResponse?> DeleteAsync<TRequest, TResponse>(string url, TRequest body, string? contentType = null, IDictionary<string, string>? headers = null, CancellationToken ct = default)
            => SendAsync<TResponse>(new HttpRequestOptions { Method = HttpMethod.Delete, Url = url, Body = body, BodyContentType = contentType, Headers = headers }, ct);

        public Task PostAsync(string url, object body, string? contentType = null, IDictionary<string, string>? headers = null, CancellationToken ct = default)
            => SendNoBodyAsync(HttpMethod.Post, url, body, contentType, headers, ct);

        public Task PutAsync(string url, object body, string? contentType = null, IDictionary<string, string>? headers = null, CancellationToken ct = default)
            => SendNoBodyAsync(HttpMethod.Put, url, body, contentType, headers, ct);

        public Task PatchAsync(string url, object body, string? contentType = null, IDictionary<string, string>? headers = null, CancellationToken ct = default) 
            => SendNoBodyAsync(new HttpMethod("PATCH"), url, body, contentType, headers, ct);

        public Task DeleteAsync(string url, IDictionary<string, string>? headers = null, CancellationToken ct = default)
            => SendNoBodyAsync(HttpMethod.Delete, url, null, null, headers, ct);

        public async Task<T?> SendAsync<T>(HttpRequestOptions options, CancellationToken ct = default)
        {
            using HttpRequestMessage req = new (options.Method, options.Url);

            if (options.Headers is not null)
            {
                foreach (KeyValuePair<string, string> kv in options.Headers)
                {
                    req.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }

            if (typeof(T) != typeof(string))
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (AllowsBody(options.Method, hasBody: options.Body is not null) && options.Body is not null)
            {
                if (options.Body is HttpContent direct)
                {
                    req.Content = direct;
                }
                else if (options.Body is string stringBody)
                {
                    string media = options.BodyContentType ?? "text/plain";
                    req.Content = new StringContent(stringBody, Utf8NoBom, media);
                }
                else
                {
                    string media = options.BodyContentType ?? _serializer.MediaType;
                    string json = _serializer.Serialize(options.Body);
                    req.Content = new StringContent(json, Utf8NoBom, media);
                }
            }

            using HttpResponseMessage resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

            if (!resp.IsSuccessStatusCode)
            {
                string? snippet = await ReadErrorSnippet(resp, ct);

                _logger?.LogWarning("HTTP {Method} {Url} -> {Status} ({Code}). Reason: {Reason}. Snippet: {Snippet}",
                    options.Method.Method, options.Url, resp.StatusCode, (int)resp.StatusCode, resp.ReasonPhrase, snippet);

                HttpRequestFailedException ex = new (new Uri(options.Url), resp.StatusCode, resp.ReasonPhrase, snippet);

                throw ex;
            }

            // Есть TResponse == string, то вернуть строку как есть
            if (typeof(T) == typeof(string))
            {
                string text = await resp.Content.ReadAsStringAsync(ct);
                return (T)(object)text;
            }

            await using Stream stream = await resp.Content.ReadAsStreamAsync(ct);

            T? result = _serializer.Deserialize<T>(stream, ct);
            return result;
        }

        private async Task SendNoBodyAsync(HttpMethod method, string url, object? body, string? contentType, IDictionary<string, string>? headers, CancellationToken ct)
        {
            using HttpRequestMessage req = new (method, url);

            if (headers is not null)
            {
                foreach (KeyValuePair<string, string> kv in headers)
                {
                    req.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                }
            }

            if (body is not null && method != HttpMethod.Get && method != HttpMethod.Head)
            {
                if (body is HttpContent direct)
                {
                    req.Content = direct;
                }
                else if (body is string s)
                {
                    string media = contentType ?? "text/plain";
                    req.Content = new StringContent(s, Utf8NoBom, media);
                }
                else
                {
                    string media = contentType ?? _serializer.MediaType;
                    string payload = _serializer.Serialize(body);
                    req.Content = new StringContent(payload, Utf8NoBom, media);
                }
            }

            using HttpResponseMessage resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

            if (!resp.IsSuccessStatusCode)
            {
                string? snippet = await ReadErrorSnippet(resp, ct);
                _logger?.LogWarning("HTTP {Method} {Url} -> {Status} ({Code}). Reason: {Reason}. Snippet: {Snippet}",
                    method.Method, url, resp.StatusCode, (int)resp.StatusCode, resp.ReasonPhrase, snippet);

                HttpRequestFailedException ex = new (new Uri(url), resp.StatusCode, resp.ReasonPhrase, snippet);

                throw ex;
            }
        }

        private static bool AllowsBody(HttpMethod method, bool hasBody)
        {
            if (method == HttpMethod.Get || method == HttpMethod.Head)
                return false;

            return hasBody || method == HttpMethod.Post || method == HttpMethod.Put || method == Patch || method == HttpMethod.Delete;
        }

        private static async Task<string?> ReadErrorSnippet(HttpResponseMessage resp, CancellationToken ct)
        {
            MediaTypeHeaderValue? mt = resp.Content.Headers.ContentType;
            bool isText =
                mt == null || mt.MediaType == null ||
                mt.MediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(mt.MediaType, "application/json", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(mt.MediaType, "application/problem+json", StringComparison.OrdinalIgnoreCase);

            if (!isText)
                return null;

            await using Stream stream = await resp.Content.ReadAsStreamAsync(ct);
            using StreamReader sr = new (stream, Encoding.UTF8, true, 4096, leaveOpen: false);

            char[] buf = new char[ERROR_SNIPPET_MAX_CHARS];
            int read = await sr.ReadBlockAsync(buf, 0, buf.Length);
            string snippet = new (buf, 0, read);
            return snippet.Replace("\r", " ").Replace("\n", " ");
        }
    }
}
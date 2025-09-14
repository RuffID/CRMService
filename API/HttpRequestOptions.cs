namespace CRMService.API
{
    public class HttpRequestOptions
    {
        public HttpMethod Method { get; init; } = HttpMethod.Get;
        public string Url { get; init; } = string.Empty;
        public IDictionary<string, string>? Headers { get; init; }
        public object? Body { get; init; }
        public string? BodyContentType { get; init; }
    }
}
using System.Net;

namespace CRMService.Models.Exceptions
{
    public sealed class HttpRequestFailedException : Exception
    {
        public Uri Url { get; }
        public HttpStatusCode StatusCode { get; }
        public string? Reason { get; }
        public string? ResponseSnippet { get; }

        public HttpRequestFailedException(Uri url, HttpStatusCode statusCode, string? reason, string? snippet) : base($"HTTP {(int)statusCode} {statusCode} for {url}. {reason}")
        {
            Url = url;
            StatusCode = statusCode;
            Reason = reason;
            ResponseSnippet = snippet;
        }
    }
}
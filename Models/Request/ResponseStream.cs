namespace CRMService.Models.Request
{
    public class ResponseStream(HttpResponseMessage response, Stream stream) : IDisposable, IAsyncDisposable
    {
        public Stream Stream { get; } = stream;
        private readonly HttpResponseMessage _response = response;
        private bool _disposed;

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                await Stream.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                _response.Dispose();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                Stream.Dispose();
            }
            finally
            {
                _response.Dispose();
            }
        }
    }
}
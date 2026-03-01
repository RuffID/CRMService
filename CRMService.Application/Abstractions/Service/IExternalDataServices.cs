using System.Collections;
using System.Data;

namespace CRMService.Application.Abstractions.Service
{
    public interface IOkdeskEntityRequestService
    {
        IAsyncEnumerable<List<T>> GetAllItemsAsync<T>(string link, long startIndex, long limit, long pageNubmer = 0, CancellationToken ct = default);
        Task<List<T>> GetRangeOfItemsAsync<T>(string link, long startIndex = 0, long limit = 0, long pageNubmer = 0, CancellationToken ct = default);
        Task<T?> GetItemAsync<T>(string link, CancellationToken ct = default);
    }

    public interface postgresSelect
    {
        Task<DataSet> Select(string sqlCommand, CancellationToken ct);
    }
}





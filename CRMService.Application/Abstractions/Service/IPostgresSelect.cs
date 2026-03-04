using System.Data;

namespace CRMService.Application.Abstractions.Service
{
    public interface IPostgresSelect
    {
        Task<DataSet> Select(string sqlCommand, CancellationToken ct);
    }
}
using CRMService.Application.Abstractions.Service;
using Npgsql;
using System.Data;

namespace CRMService.Infrastructure.DataBase.Postgresql
{
    public class PGSelect(PGConfig pgConfig, ILoggerFactory logger) : IPostgresSelect
    {
        private readonly ILogger<PGSelect> _logger = logger.CreateLogger<PGSelect>();

        public async Task<DataSet> Select(string sqlCommand, CancellationToken ct)
        {
            using NpgsqlConnection connection = pgConfig.GetPsqlConnection();
            DataSet dataSet = new();
            try
            {
                await connection.OpenAsync(ct);
                using NpgsqlDataAdapter adapter = new(sqlCommand, connection);
                adapter.Fill(dataSet);
                return dataSet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Method:{MethodName}] Error executing select query.", nameof(Select));
                return dataSet;
            }
            finally
            {
                await connection.CloseAsync();
                await connection.DisposeAsync();
            }
        }
    }
}
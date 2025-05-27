using CRMService.Models.ConfigClass;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace CRMService.DataBase.Postgresql
{
    public class PGSelect(IOptions<DatabaseSettings> db, ILoggerFactory logger)
    {
        private readonly PGConfig _config = new(db);
        private readonly ILogger<PGSelect> _logger = logger.CreateLogger<PGSelect>();

        public async Task<DataSet> Select(string sqlCommand)
        {
            using NpgsqlConnection connection = _config.GetPsqlConnection();
            DataSet dataSet = new();
            try
            {
                await connection.OpenAsync();
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

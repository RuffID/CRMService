using Npgsql;

namespace CRMService.Infrastructure.DataBase.Postgresql
{
    public class PGConfig(string connectionString)
    {
        public NpgsqlConnection GetPsqlConnection() => new (connectionString);
    }
}
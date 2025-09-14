using Npgsql;

namespace CRMService.DataBase.Postgresql
{
    public class PGConfig(string connectionString)
    {
        public NpgsqlConnection GetPsqlConnection() => new (connectionString);
    }
}


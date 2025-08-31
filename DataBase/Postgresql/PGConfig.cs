using Npgsql;

namespace CRMService.DataBase.Postgresql
{
    public class PGConfig(string connectionString)
    {
        private readonly string host = "localhost";
        private readonly int port = 5432;
        private readonly string database = "postgres";
        private readonly string username = "user";
        private readonly string password = "postgres";

        public NpgsqlConnection GetPsqlConnection()
        {
            return GetPsqlConnection(host, port, database, username, password);
        }

        private NpgsqlConnection GetPsqlConnection(string host, int port, string database, string username, string password)
        {
            // Connection String.
            if (string.IsNullOrEmpty(connectionString))
            {
                return new NpgsqlConnection
                    ("host=" + host + ";database=" + database + ";port=" + port + ";username=" + username + ";password=" + password);
            }
            else return new NpgsqlConnection(connectionString);
        }
    }
}


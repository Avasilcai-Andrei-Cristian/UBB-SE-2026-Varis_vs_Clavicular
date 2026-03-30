using Microsoft.Data.SqlClient;

namespace matchmaking.Repositories;

public class SqlConnectionTestRepository(string connectionString) : SqlRepositoryBase(connectionString)
{
    public int Ping()
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand("SELECT 1", connection);
        return (int)(command.ExecuteScalar() ?? 0);
    }
}

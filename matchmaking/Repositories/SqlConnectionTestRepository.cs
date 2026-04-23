using Microsoft.Data.SqlClient;

namespace matchmaking.Repositories;

public class SqlConnectionTestRepository : SqlRepositoryBase, IConnectionTestRepository
{
    public SqlConnectionTestRepository(string connectionString)
        : base(connectionString)
    {
    }

    public int Ping()
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand("SELECT 1", connection);
        return (int)(command.ExecuteScalar() ?? 0);
    }
}

using Microsoft.Data.SqlClient;
using Xunit;

namespace matchmaking.Tests;

[CollectionDefinition("SqlIntegration", DisableParallelization = true)]
public sealed class SqlIntegrationCollection : ICollectionFixture<SqlIntegrationTestDatabaseFixture>
{
}

public sealed class SqlIntegrationTestDatabaseFixture : IDisposable
{
    public SqlIntegrationTestDatabaseFixture()
    {
        try
        {
            Database = SqlIntegrationTestDatabase.Create();
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Failed to initialize SQL integration test database. {exception.Message}", exception);
        }
    }

    public SqlIntegrationTestDatabase Database { get; }

    public void Dispose()
    {
        Database.Dispose();
    }
}

public sealed class SqlIntegrationTestDatabase : IDisposable
{
    private readonly string masterConnectionString;
    private readonly string databaseName;

    private SqlIntegrationTestDatabase(string masterConnectionString, string databaseName, string connectionString)
    {
        this.masterConnectionString = masterConnectionString;
        this.databaseName = databaseName;
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; }

    public static SqlIntegrationTestDatabase Create()
    {
        var masterConnectionString = Environment.GetEnvironmentVariable("MATCHMAKING_TEST_MASTER_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(masterConnectionString))
        {
            masterConnectionString = "Server=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=true;TrustServerCertificate=true";
        }

        var databaseName = $"matchmaking_test_{Guid.NewGuid():N}";

        using (var masterConnection = new SqlConnection(masterConnectionString))
        {
            masterConnection.Open();
            using var createCommand = new SqlCommand($"CREATE DATABASE [{databaseName}]", masterConnection);
            createCommand.ExecuteNonQuery();
        }

        var testConnectionString = BuildTestConnectionString(masterConnectionString, databaseName);
        var database = new SqlIntegrationTestDatabase(masterConnectionString, databaseName, testConnectionString);
        database.EnsureSchema();
        return database;
    }

    public void ResetData()
    {
        ExecuteNonQuery(
            "DELETE FROM Message; DELETE FROM Interaction; DELETE FROM Recommendation; DELETE FROM Matches; DELETE FROM Chat; DELETE FROM Post; DELETE FROM Developer;");

        ExecuteNonQuery("DBCC CHECKIDENT ('Message', RESEED, 0);");
        ExecuteNonQuery("DBCC CHECKIDENT ('Interaction', RESEED, 0);");
        ExecuteNonQuery("DBCC CHECKIDENT ('Recommendation', RESEED, 0);");
        ExecuteNonQuery("DBCC CHECKIDENT ('Matches', RESEED, 0);");
        ExecuteNonQuery("DBCC CHECKIDENT ('Chat', RESEED, 0);");
        ExecuteNonQuery("DBCC CHECKIDENT ('Post', RESEED, 0);");
        ExecuteNonQuery("DBCC CHECKIDENT ('Developer', RESEED, 0);");
    }

    public int ExecuteNonQuery(string sql, Action<SqlParameterCollection>? configureParameters = null)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);
        configureParameters?.Invoke(command.Parameters);
        return command.ExecuteNonQuery();
    }

    public T ExecuteScalar<T>(string sql, Action<SqlParameterCollection>? configureParameters = null)
    {
        using var connection = new SqlConnection(ConnectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);
        configureParameters?.Invoke(command.Parameters);
        var result = command.ExecuteScalar();
        if (result is null or DBNull)
        {
            throw new InvalidOperationException("Expected scalar result but found null.");
        }

        return (T)Convert.ChangeType(result, typeof(T));
    }

    public void Dispose()
    {
        using var masterConnection = new SqlConnection(masterConnectionString);
        masterConnection.Open();
        using var command = new SqlCommand(
            $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}];",
            masterConnection);
        command.ExecuteNonQuery();
    }

    private static string BuildTestConnectionString(string masterConnectionString, string databaseName)
    {
        var builder = new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = databaseName,
            Pooling = false,
            TrustServerCertificate = true
        };
        return builder.ConnectionString;
    }

    private void EnsureSchema()
    {
        ExecuteNonQuery(
            """
            CREATE TABLE Developer (
                DeveloperID INT IDENTITY(1,1) NOT NULL,
                Name NVARCHAR(255) NOT NULL,
                Password NVARCHAR(255) NOT NULL,
                CONSTRAINT PK_Developer PRIMARY KEY (DeveloperID)
            );

            CREATE TABLE Post (
                PostID INT IDENTITY(1,1) NOT NULL,
                DeveloperID INT NOT NULL,
                Parameter NVARCHAR(255) NOT NULL,
                Value NVARCHAR(255) NOT NULL,
                CONSTRAINT PK_Post PRIMARY KEY (PostID),
                CONSTRAINT FK_Post_Dev FOREIGN KEY (DeveloperID) REFERENCES Developer(DeveloperID)
            );

            CREATE TABLE Interaction (
                InteractionID INT IDENTITY(1,1) NOT NULL,
                DeveloperID INT NOT NULL,
                PostID INT NOT NULL,
                Type BIT NOT NULL,
                CONSTRAINT PK_Interaction PRIMARY KEY (InteractionID),
                CONSTRAINT FK_Inter_Developer FOREIGN KEY (DeveloperID) REFERENCES Developer(DeveloperID),
                CONSTRAINT FK_Inter_Post FOREIGN KEY (PostID) REFERENCES Post(PostID)
            );

            CREATE TABLE Chat (
                ChatId INT IDENTITY(1,1) NOT NULL,
                UserId INT NOT NULL,
                CompanyId INT NULL,
                SecondUserId INT NULL,
                JobId INT NULL,
                IsBlocked BIT NOT NULL DEFAULT 0,
                BlockedByUserId INT NULL,
                DeletedAtByUser DATETIME2(7) NULL,
                DeletedAtBySecondParty DATETIME2(7) NULL,
                CONSTRAINT PK_Chat PRIMARY KEY (ChatId)
            );

            CREATE TABLE Message (
                MessageID INT IDENTITY(1,1) NOT NULL,
                Content NVARCHAR(MAX) NOT NULL,
                SenderID INT NOT NULL,
                Timestamp DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
                ChatId INT NOT NULL,
                Type TINYINT NOT NULL,
                IsRead BIT NOT NULL DEFAULT 0,
                CONSTRAINT PK_Message PRIMARY KEY (MessageID),
                CONSTRAINT FK_Message_Chat FOREIGN KEY (ChatId) REFERENCES Chat(ChatId)
            );

            CREATE TABLE Matches (
                MatchID INT IDENTITY(1,1) NOT NULL,
                UserID INT NOT NULL,
                JobID INT NOT NULL,
                Status VARCHAR(50) NOT NULL,
                Timestamp DATETIME2(7) NOT NULL,
                Feedback NVARCHAR(MAX) NULL,
                CONSTRAINT PK_Match PRIMARY KEY (MatchID)
            );

            CREATE TABLE Recommendation (
                RecommendationId INT IDENTITY(1,1) NOT NULL,
                UserID INT NOT NULL,
                JobID INT NOT NULL,
                Timestamp DATETIME2(7) NOT NULL,
                CONSTRAINT PK_Recommendation PRIMARY KEY (RecommendationId)
            );
            """);
    }
}

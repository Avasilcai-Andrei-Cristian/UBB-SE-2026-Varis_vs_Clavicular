namespace matchmaking.Tests;

[CollectionDefinition("SqlIntegration", DisableParallelization = true)]
public sealed class SqlIntegrationCollection : ICollectionFixture<SqlIntegrationTestDatabase>
{
}

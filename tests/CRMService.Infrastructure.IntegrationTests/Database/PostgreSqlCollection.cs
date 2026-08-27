using Xunit;

namespace CRMService.Infrastructure.IntegrationTests.Database;

[CollectionDefinition(Name)]
public class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "PostgreSQL";
}

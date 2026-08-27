using Xunit;

namespace CRMService.Infrastructure.IntegrationTests.Database;

[CollectionDefinition(Name)]
public class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SQL Server";
}

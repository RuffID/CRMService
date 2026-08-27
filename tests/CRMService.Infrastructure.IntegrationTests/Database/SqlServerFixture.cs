using CRMService.Infrastructure.DataBase;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Respawn.Graph;
using Testcontainers.MsSql;
using Xunit;

namespace CRMService.Infrastructure.IntegrationTests.Database;

public class SqlServerFixture : IAsyncLifetime
{
    private readonly string _databaseName = $"crm_stage05_{Guid.NewGuid():N}";
    private readonly MsSqlContainer _container;
    private Respawner? _respawner;

    public SqlServerFixture()
    {
        string containerName = $"crm-stage05-sql-{Guid.NewGuid():N}";
        _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04")
            .WithName(containerName)
            .WithPassword("Stage05_Strong!Password_2026")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .Build();
    }

    public string ConnectionString
    {
        get
        {
            SqlConnectionStringBuilder builder = new(_container.GetConnectionString())
            {
                InitialCatalog = _databaseName,
                TrustServerCertificate = true
            };
            return builder.ConnectionString;
        }
    }

    public MainContext CreateContext()
    {
        DbContextOptions<MainContext> options = new DbContextOptionsBuilder<MainContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new MainContext(options);
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken);

        await using MainContext context = CreateContext();
        await context.Database.MigrateAsync(TestContext.Current.CancellationToken);

        await using SqlConnection connection = new(ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = [new Table("__EFMigrationsHistory")]
        });
    }

    public async Task ResetAsync(CancellationToken ct)
    {
        if (_respawner is null)
            throw new InvalidOperationException("SQL Server fixture is not initialized.");

        await using SqlConnection connection = new(ConnectionString);
        await connection.OpenAsync(ct);
        await _respawner.ResetAsync(connection);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _container.StopAsync();
        }
        finally
        {
            await _container.DisposeAsync();
        }
    }
}

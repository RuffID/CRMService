using CRMService.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

namespace CRMService.Infrastructure.IntegrationTests.Database;

public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private Respawner? _respawner;

    public PostgreSqlFixture()
    {
        string databaseName = $"crm_stage05_{Guid.NewGuid():N}";
        string containerName = $"crm-stage05-pg-{Guid.NewGuid():N}";
        _container = new PostgreSqlBuilder("postgres:17.6-bookworm")
            .WithName(containerName)
            .WithDatabase(databaseName)
            .WithUsername("stage05")
            .WithPassword("Stage05_Postgres!2026")
            .Build();
    }

    public string ConnectionString => _container.GetConnectionString();

    public OkdeskContext CreateContext()
    {
        DbContextOptions<OkdeskContext> options = new DbContextOptionsBuilder<OkdeskContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new OkdeskContext(options);
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken);

        await using OkdeskContext context = CreateContext();
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

        await using NpgsqlConnection connection = new(ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetAsync(CancellationToken ct)
    {
        if (_respawner is null)
            throw new InvalidOperationException("PostgreSQL fixture is not initialized.");

        await using NpgsqlConnection connection = new(ConnectionString);
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

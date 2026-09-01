using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Application.Abstractions.Database.Repository.Entity;
using CRMService.Application.Abstractions.Database.Repository.Report;
using CRMService.Domain.Models.Authorization;
using CRMService.Domain.Models.OkdeskEntity;
using CRMService.Infrastructure.DataBase;
using CRMService.Infrastructure.Service.DataBase;
using EFCoreLibrary.EfCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CRMService.Infrastructure.IntegrationTests.Database;

[Collection(SqlServerCollection.Name)]
[Trait("Dependency", "Docker")]
public class SqlServerInfrastructureTests(SqlServerFixture fixture)
{
    [Fact]
    public async Task MigrateAsync_CleanDatabase_AppliesEveryProductionMigration()
    {
        await using MainContext context = fixture.CreateContext();

        IReadOnlyList<string> defined = context.Database.GetMigrations().ToList();
        IReadOnlyList<string> applied = (await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken)).ToList();
        IReadOnlyList<string> pending = (await context.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken)).ToList();

        Assert.NotEmpty(defined);
        Assert.Equal(defined, applied);
        Assert.Empty(pending);
    }

    [Fact]
    public async Task MigrateAsync_IdentityTables_GenerateKeysAfterFullMigrationChain()
    {
        await fixture.ResetAsync(TestContext.Current.CancellationToken);
        await using MainContext context = fixture.CreateContext();
        IssuePriority priority = new() { Code = "stage05-priority", Name = "Stage 05 priority" };
        IssueStatus status = new() { Code = "stage05-status", Name = "Stage 05 status" };
        context.AddRange(priority, status);

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.True(priority.Id > 0);
        Assert.True(status.Id > 0);
    }

    [Fact]
    public async Task AuthorizationUnitOfWork_RepositoryAndSaveChanges_UseOneChangeTracker()
    {
        await fixture.ResetAsync(TestContext.Current.CancellationToken);
        await using ServiceProvider provider = MainTestServices.Create(fixture.ConnectionString);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IAuthorizationUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IAuthorizationUnitOfWork>();
        MainContext context = scope.ServiceProvider.GetRequiredService<MainContext>();
        User user = new() { Login = "stage05", Name = "Stage 05", Password = "hash", Active = true };

        unitOfWork.User.Create(user);
        Assert.Same(user, context.ChangeTracker.Entries<User>().Single().Entity);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        context.ChangeTracker.Clear();
        User? saved = await unitOfWork.User.GetItemByIdAsync(user.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(saved);
        Assert.Equal(EntityState.Unchanged, context.Entry(saved).State);
        context.ChangeTracker.Clear();
        User? readOnly = await unitOfWork.User.GetItemByIdReadOnlyAsync(user.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(readOnly);
        Assert.Empty(context.ChangeTracker.Entries<User>());
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_Success_CommitsChanges()
    {
        await fixture.ResetAsync(TestContext.Current.CancellationToken);
        await using ServiceProvider provider = MainTestServices.Create(fixture.ConnectionString);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IAuthorizationUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IAuthorizationUnitOfWork>();
        MainContext context = scope.ServiceProvider.GetRequiredService<MainContext>();

        await unitOfWork.ExecuteInTransactionAsync(
            _ =>
            {
                unitOfWork.User.Create(new User { Login = "committed", Name = "Committed", Password = "hash", Active = true });
                return Task.CompletedTask;
            },
            TestContext.Current.CancellationToken);

        context.ChangeTracker.Clear();
        Assert.NotNull(await unitOfWork.User.GetByLoginReadOnlyAsync("committed", ct: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_Failure_RollsBackChanges()
    {
        await fixture.ResetAsync(TestContext.Current.CancellationToken);
        await using ServiceProvider provider = MainTestServices.Create(fixture.ConnectionString);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IAuthorizationUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IAuthorizationUnitOfWork>();
        MainContext context = scope.ServiceProvider.GetRequiredService<MainContext>();

        await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                unitOfWork.User.Create(new User { Login = "rolled-back", Name = "Rolled back", Password = "hash", Active = true });
                await unitOfWork.SaveChangesAsync(ct);
                throw new InvalidOperationException("rollback");
            },
            TestContext.Current.CancellationToken));

        context.ChangeTracker.Clear();
        Assert.Null(await unitOfWork.User.GetByLoginReadOnlyAsync("rolled-back", ct: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SaveChangesAsync_CancelledToken_PropagatesCancellation()
    {
        await fixture.ResetAsync(TestContext.Current.CancellationToken);
        await using ServiceProvider provider = MainTestServices.Create(fixture.ConnectionString);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IAuthorizationUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IAuthorizationUnitOfWork>();
        unitOfWork.User.Create(new User { Login = "cancelled", Name = "Cancelled", Password = "hash", Active = true });
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => unitOfWork.SaveChangesAsync(cancellation.Token));
    }

    [Fact]
    public async Task IssueRepository_FilterPaginationAndReadOnlyQuery_ReturnExpectedRows()
    {
        await fixture.ResetAsync(TestContext.Current.CancellationToken);
        await using ServiceProvider provider = MainTestServices.Create(fixture.ConnectionString);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        MainContext context = scope.ServiceProvider.GetRequiredService<MainContext>();
        IIssueRepository repository = scope.ServiceProvider.GetRequiredService<IIssueRepository>();
        DateTime boundary = new(2026, 8, 27, 0, 0, 0, DateTimeKind.Utc);
        context.Issues.AddRange(
            new Issue { Id = 3, Title = "third", CreatedAt = boundary, EmployeesUpdatedAt = boundary },
            new Issue { Id = 1, Title = "first", CreatedAt = boundary, EmployeesUpdatedAt = boundary },
            new Issue { Id = 2, Title = "second", CreatedAt = boundary, EmployeesUpdatedAt = boundary.AddDays(-1) },
            new Issue { Id = -1, Title = "deleted", CreatedAt = boundary, EmployeesUpdatedAt = boundary, DeletedAt = boundary });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        context.ChangeTracker.Clear();

        List<Issue> page = await repository.GetFromIdReadOnlyAsync(1, 2, TestContext.Current.CancellationToken);
        List<Issue> updated = await repository.GetUpdatedLocalReadOnlyAsync(boundary, boundary, TestContext.Current.CancellationToken);

        Assert.Equal([1, 2], page.Select(x => x.Id));
        Assert.Equal([1, 3], updated.OrderBy(x => x.Id).Select(x => x.Id));
        Assert.Empty(context.ChangeTracker.Entries<Issue>());
    }

    [Fact]
    public async Task ReportRepositories_InclusiveBoundariesAndEmptySelection_ReturnExpectedAggregates()
    {
        await fixture.ResetAsync(TestContext.Current.CancellationToken);
        await using ServiceProvider provider = MainTestServices.Create(fixture.ConnectionString);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        MainContext context = scope.ServiceProvider.GetRequiredService<MainContext>();
        IIssueDynamicsChartReportRepository repository = scope.ServiceProvider.GetRequiredService<IIssueDynamicsChartReportRepository>();
        DateTime from = new(2026, 8, 27, 10, 0, 0);
        DateTime to = from.AddHours(1);
        context.Issues.AddRange(
            new Issue { Id = 1, Title = "from", CreatedAt = from, EmployeesUpdatedAt = from },
            new Issue { Id = 2, Title = "to", CreatedAt = to, EmployeesUpdatedAt = to });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var points = await repository.GetCreatedIssuesAsync(from, to, "hour", TestContext.Current.CancellationToken);
        var empty = await repository.GetCreatedIssuesAsync(to.AddSeconds(1), to.AddHours(1), "hour", TestContext.Current.CancellationToken);

        Assert.Equal(2, points.Sum(x => x.Count));
        Assert.Empty(empty);
    }

    [Fact]
    public async Task SqlServerBackupService_ContainerLocalPath_CreatesUniqueBackups()
    {
        SqlServerBackupService service = CreateBackupService(
            fixture.ConnectionString,
            "/var/opt/mssql/data");

        string firstPath = await service.CreateBackupAsync(TestContext.Current.CancellationToken);
        string secondPath = await service.CreateBackupAsync(TestContext.Current.CancellationToken);

        Assert.StartsWith("/var/opt/mssql/data/backup_CRMService_", firstPath, StringComparison.Ordinal);
        Assert.StartsWith("/var/opt/mssql/data/backup_CRMService_", secondPath, StringComparison.Ordinal);
        Assert.NotEqual(firstPath, secondPath);
    }

    [Fact]
    public async Task DataBaseCheckUpService_ConnectionFailure_FailsFast()
    {
        const string connectionString = "Server=127.0.0.1,1;Database=missing;User Id=sa;Password=Unused_Strong!1;TrustServerCertificate=True;Connect Timeout=1";
        DbContextOptions<MainContext> options = new DbContextOptionsBuilder<MainContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using MainContext context = new(options);
        EfDbContextAdapter<MainContext> adapter = new(context);
        SqlServerBackupService backup = CreateBackupService(connectionString, "/var/opt/mssql/data");
        DataBaseCheckUpService<MainContext> service = new(adapter, NullLoggerFactory.Instance, backup);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CheckOrUpdateDBAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DataBaseCheckUpService_CancelledToken_PropagatesCancellation()
    {
        await using MainContext context = fixture.CreateContext();
        EfDbContextAdapter<MainContext> adapter = new(context);
        SqlServerBackupService backup = CreateBackupService(
            fixture.ConnectionString,
            "/var/opt/mssql/data");
        DataBaseCheckUpService<MainContext> service = new(adapter, NullLoggerFactory.Instance, backup);
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.CheckOrUpdateDBAsync(cancellation.Token));
    }

    [Fact]
    public async Task DataBaseCheckUpService_ContextWithoutMigrations_DoesNotCreateBackup()
    {
        SqlConnectionStringBuilder connection = new(fixture.ConnectionString)
        {
            InitialCatalog = $"no_migrations_{Guid.NewGuid():N}"
        };
        DbContextOptions<NoMigrationsContext> options = new DbContextOptionsBuilder<NoMigrationsContext>()
            .UseSqlServer(connection.ConnectionString)
            .Options;
        await using NoMigrationsContext context = new(options);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        EfDbContextAdapter<NoMigrationsContext> adapter = new(context);
        SqlServerBackupService backup = CreateBackupService(
            connection.ConnectionString,
            "/path/that/must/not/be/used");
        DataBaseCheckUpService<NoMigrationsContext> service = new(adapter, NullLoggerFactory.Instance, backup);

        await service.CheckOrUpdateDBAsync(TestContext.Current.CancellationToken);

        Assert.Empty(context.Database.GetMigrations());
    }

    [Fact]
    public async Task DataBaseCheckUpService_PendingMigration_CreatesBackupAndAppliesMigration()
    {
        SqlConnectionStringBuilder connection = new(fixture.ConnectionString)
        {
            InitialCatalog = $"pending_migration_{Guid.NewGuid():N}"
        };
        DbContextOptions<PendingMigrationContext> options = new DbContextOptionsBuilder<PendingMigrationContext>()
            .UseSqlServer(
                connection.ConnectionString,
                sqlServer => sqlServer.MigrationsAssembly(typeof(TestPendingMigration).Assembly.FullName))
            .Options;
        await using PendingMigrationContext context = new(options);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        Assert.Contains(
            "20260827000000_TestPendingMigration",
            await context.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken));
        EfDbContextAdapter<PendingMigrationContext> adapter = new(context);
        SqlServerBackupService backup = CreateBackupService(
            connection.ConnectionString,
            "/var/opt/mssql/data");
        DataBaseCheckUpService<PendingMigrationContext> service = new(adapter, NullLoggerFactory.Instance, backup);

        await service.CheckOrUpdateDBAsync(TestContext.Current.CancellationToken);

        Assert.Contains(
            "20260827000000_TestPendingMigration",
            await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken));
        Assert.Empty(await context.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken));
    }

    private static SqlServerBackupService CreateBackupService(
        string connectionString,
        string sqlServerPath)
    {
        DatabaseBackupOptions options = new()
        {
            ConnectionString = connectionString,
            ProjectName = "CRMService",
            WindowsSqlServerPath = sqlServerPath,
            LinuxSqlServerPath = sqlServerPath
        };

        return new SqlServerBackupService(
            Options.Create(options),
            NullLogger<SqlServerBackupService>.Instance);
    }

    private class NoMigrationsContext(DbContextOptions<NoMigrationsContext> options) : DbContext(options);
}

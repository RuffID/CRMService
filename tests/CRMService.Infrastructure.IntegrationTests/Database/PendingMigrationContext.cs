using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.IntegrationTests.Database;

public class PendingMigrationContext(DbContextOptions<PendingMigrationContext> options) : DbContext(options);

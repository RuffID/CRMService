using CRMService.Models.ConfigClass;
using CRMService.Models.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CRMService.DataBase;

public partial class CRMServerInfoContext : DbContext
{
    private readonly DatabaseSettings _dbSettings;

    public CRMServerInfoContext(IOptions<DatabaseSettings> dbSetings)
    {
        _dbSettings = dbSetings.Value;
        Database.EnsureCreated();
    }

    public virtual DbSet<ClientAppInfo> Clients { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(_dbSettings.MySqlServerInfoCRM ?? "",
                new MySqlServerVersion(new Version(_dbSettings.MySqlVersion ?? "")));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<ClientAppInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("client");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            entity.Property(e => e.ReleaseDate)
                .HasColumnType("datetime")
                .HasColumnName("release_date");
            entity.Property(e => e.Version)
                .HasMaxLength(45)
                .HasColumnName("version");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using Microsoft.EntityFrameworkCore;

namespace CRMService.Infrastructure.DataBase
{
    public sealed class OkdeskContext : DbContext
    {
        public OkdeskContext(DbContextOptions<OkdeskContext> options) : base(options) { }

        public DbSet<IssueStatusOkdesk> IssueStatuses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IssueStatusOkdesk>(entity =>
            {
                entity.ToTable("issue_statuses");
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.Code).HasColumnName("code");
                entity.Property(x => x.Name).HasColumnName("name");
                entity.Property(x => x.Final).HasColumnName("final");
                entity.Property(x => x.KeepDeadline).HasColumnName("keep_deadline");
            });
        }
    }

    public sealed class IssueStatusOkdesk
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Final { get; set; }
        public bool KeepDeadline { get; set; }
    }
}




using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class TimeEntryConfigure : IEntityTypeConfiguration<TimeEntry>
    {
        public void Configure(EntityTypeBuilder<TimeEntry> builder)
        {
            builder.ToTable("time_entry");

            builder.HasKey(e => e.Id).HasName("PRIMARY");

            builder.HasIndex(e => e.EmployeeId, "employeeId_idx");

            builder.HasIndex(e => e.IssueId, "issueId_idx");

            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            builder.Property(e => e.EmployeeId)
                .HasColumnName("employeeId");

            builder.Property(e => e.IssueId)
                .HasColumnName("issueId");

            builder.Property(e => e.LoggedAt)
                .HasColumnType("datetime")
                .HasColumnName("logged_at");

            builder.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            builder.Property(e => e.SpentTime)
                .HasColumnName("spentTime");

            builder.HasOne(d => d.Employee)
                .WithMany(p => p.TimeEntries)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("employeeId");

            builder.HasOne(d => d.Issue)
                .WithMany(p => p.TimeEntries)
                .HasForeignKey(d => d.IssueId)
                .OnDelete(DeleteBehavior.Cascade)                
                .HasConstraintName("issueId");
        }
    }
}

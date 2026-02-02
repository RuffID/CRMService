using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class TimeEntryConfigure : IEntityTypeConfiguration<TimeEntry>
    {
        public void Configure(EntityTypeBuilder<TimeEntry> builder)
        {
            builder.ToTable("TimeEntry");            

            builder.HasIndex(e => e.EmployeeId, "employeeId_idx");

            builder.HasIndex(e => e.IssueId, "issueId_idx");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.HasOne(d => d.Employee)
                .WithMany(p => p.TimeEntries)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.Issue)
                .WithMany(p => p.TimeEntries)
                .HasForeignKey(d => d.IssueId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

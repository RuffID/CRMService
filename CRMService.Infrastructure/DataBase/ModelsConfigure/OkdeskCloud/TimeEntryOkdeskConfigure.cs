using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class TimeEntryOkdeskConfigure : IEntityTypeConfiguration<TimeEntry>
    {
        public void Configure(EntityTypeBuilder<TimeEntry> builder)
        {
            builder.ToTable("time_entries");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.SpentTime).HasColumnName("spent_time");
            builder.Property(x => x.LoggedAt).HasColumnName("logged_at");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at");           

            builder.Property<int>("EmployeeInternalId").HasColumnName("employee_id");
            builder.Property<int>("IssueInternalId").HasColumnName("issue_id");

            builder.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey("EmployeeInternalId")
                .HasPrincipalKey("InternalId")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Issue)
                .WithMany()
                .HasForeignKey("IssueInternalId")
                .HasPrincipalKey("InternalId")
                .OnDelete(DeleteBehavior.NoAction);

            builder.Ignore(x => x.EmployeeId);
            builder.Ignore(x => x.IssueId);
        }
    }
}

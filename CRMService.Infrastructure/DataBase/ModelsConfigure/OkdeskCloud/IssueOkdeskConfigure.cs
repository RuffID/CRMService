using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class IssueOkdeskConfigure : IEntityTypeConfiguration<Issue>
    {
        public void Configure(EntityTypeBuilder<Issue> builder)
        {
            builder.ToTable("issues");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("sequential_id");
            builder.Property<int>("InternalId").HasColumnName("id");
            builder.HasAlternateKey("InternalId");
            builder.Property(x => x.Title).HasColumnName("title");
            builder.Property(x => x.EmployeesUpdatedAt).HasColumnName("employees_updated_at");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at");
            builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
            builder.Property(x => x.DeadlineAt).HasColumnName("deadline_at");
            builder.Property(x => x.DelayTo).HasColumnName("delay_to");
            builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");           

            builder.Property<int?>("AssigneeInternalId").HasColumnName("assignee_id");
            builder.Property<int?>("AuthorInternalId").HasColumnName("author_id");
            builder.Property<int?>("CompanyInternalId").HasColumnName("company_id");
            builder.Property<int?>("ServiceObjectInternalId").HasColumnName("maintenance_entity_id");
            builder.Property(x => x.StatusId).HasColumnName("status_id");
            builder.Property(x => x.TypeId).HasColumnName("work_type_id");
            builder.Property(x => x.PriorityId).HasColumnName("priority_id");

            builder.HasOne(x => x.Assignee)
                .WithMany()
                .HasForeignKey("AssigneeInternalId")
                .HasPrincipalKey("InternalId")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Company)
                .WithMany()
                .HasForeignKey("CompanyInternalId")
                .HasPrincipalKey("InternalId")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ServiceObject)
                .WithMany()
                .HasForeignKey("ServiceObjectInternalId")
                .HasPrincipalKey("InternalId")
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Status)
                .WithMany()
                .HasForeignKey(x => x.StatusId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Type)
                .WithMany()
                .HasForeignKey(x => x.TypeId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Priority)
                .WithMany()
                .HasForeignKey(x => x.PriorityId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Ignore(x => x.TimeEntries);
            builder.Ignore(x => x.AssigneeId);
            builder.Ignore(x => x.AuthorId);
            builder.Ignore(x => x.CompanyId);
            builder.Ignore(x => x.ServiceObjectId);
        }
    }
}

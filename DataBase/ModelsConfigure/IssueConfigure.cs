using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class IssueConfigure : IEntityTypeConfiguration<Issue>
    {
        public void Configure(EntityTypeBuilder<Issue> builder)
        {
            builder.ToTable("issue");

            builder.HasIndex(e => e.AssigneeId, "issue_assigneeId_idx");

            builder.HasIndex(e => e.CompanyId, "issue_companyId_idx");

            builder.HasIndex(e => e.PriorityId, "issue_priorityId_idx");

            builder.HasIndex(e => e.ServiceObjectId, "issue_service_objectId_idx");

            builder.HasIndex(e => e.StatusId, "issue_statusId_idx");

            builder.HasIndex(e => e.TypeId, "issue_typeId_idx");

            builder.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            builder.Property(e => e.AssigneeId).HasColumnName("assignee_id");
            builder.Property(e => e.AuthorId).HasColumnName("author_id");
            builder.Property(e => e.CompanyId).HasColumnName("companyId");

            builder.Property(e => e.CompletedAt)
                .HasColumnType("datetime")
                .HasColumnName("completed_at");

            builder.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            builder.Property(e => e.DeadlineAt)
                .HasColumnType("datetime")
                .HasColumnName("deadline_at");

            builder.Property(e => e.DelayTo)
                .HasColumnType("datetime")
                .HasColumnName("delay_to");

            builder.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");

            builder.Property(e => e.EmployeesUpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("employees_updated_at");

            builder.Property(e => e.PriorityId).HasColumnName("priorityId");
            builder.Property(e => e.ServiceObjectId).HasColumnName("service_objectId");
            builder.Property(e => e.StatusId).HasColumnName("statusId");

            builder.Property(e => e.Title)
                .HasMaxLength(3000)
                .HasColumnName("title");

            builder.Property(e => e.TypeId).HasColumnName("typeId");

            builder.HasOne(d => d.Assignee).WithMany(p => p.Issues)
                .HasForeignKey(d => d.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("issue_assigneeId");

            builder.HasOne(d => d.Company).WithMany(p => p.Issues)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("issue_companyId");

            builder.HasOne(d => d.Priority).WithMany(p => p.Issues)
                .HasForeignKey(d => d.PriorityId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("issue_priorityId");

            builder.HasOne(d => d.ServiceObject).WithMany(p => p.Issues)
                .HasForeignKey(d => d.ServiceObjectId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("issue_service_objectId");

            builder.HasOne(d => d.Status).WithMany(p => p.Issues)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("issue_statusId");

            builder.HasOne(d => d.Type).WithMany(p => p.Issues)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("issue_typeId");
        }
    }
}
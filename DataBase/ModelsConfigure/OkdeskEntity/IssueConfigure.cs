using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class IssueConfigure : IEntityTypeConfiguration<Issue>
    {
        public void Configure(EntityTypeBuilder<Issue> builder)
        {
            builder.ToTable("Issue");

            builder.HasIndex(e => e.AssigneeId, "issue_assigneeId_idx");

            builder.HasIndex(e => e.CompanyId, "issue_companyId_idx");

            builder.HasIndex(e => e.PriorityId, "issue_priorityId_idx");

            builder.HasIndex(e => e.ServiceObjectId, "issue_service_objectId_idx");

            builder.HasIndex(e => e.StatusId, "issue_statusId_idx");

            builder.HasIndex(e => e.TypeId, "issue_typeId_idx");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Title)
                .HasMaxLength(3000);

            builder.HasOne(d => d.Assignee)
                .WithMany(p => p.Issues)
                .HasForeignKey(d => d.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Company)
                .WithMany(p => p.Issues)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Priority)
                .WithMany(p => p.Issues)
                .HasForeignKey(d => d.PriorityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.ServiceObject)
                .WithMany(p => p.Issues)
                .HasForeignKey(d => d.ServiceObjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Status)
                .WithMany(p => p.Issues)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Type)
                .WithMany(p => p.Issues)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
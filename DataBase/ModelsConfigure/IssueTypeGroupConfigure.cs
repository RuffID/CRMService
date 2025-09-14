using CRMService.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure
{
    public class IssueTypeGroupConfigure : IEntityTypeConfiguration<IssueTypeGroup>
    {
        public void Configure(EntityTypeBuilder<IssueTypeGroup> builder)
        {
            builder.ToTable("issue_type_groups");

            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

            builder.Property(e => e.ParentGroupId).HasColumnName("parent_group_id");

            builder.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");

            builder.Property(e => e.Code).HasMaxLength(60).HasColumnName("code");

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
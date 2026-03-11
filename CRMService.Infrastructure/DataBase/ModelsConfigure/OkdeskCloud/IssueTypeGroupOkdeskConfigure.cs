using CRMService.Domain.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.Infrastructure.DataBase.ModelsConfigure.OkdeskCloud
{
    public class IssueTypeGroupOkdeskConfigure : IEntityTypeConfiguration<IssueTypeGroup>
    {
        public void Configure(EntityTypeBuilder<IssueTypeGroup> builder)
        {
            builder.ToTable("issue_work_type_groups");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.ParentGroupId).HasColumnName("parent_id");

            builder.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentGroupId);

            builder.HasMany(x => x.Types)
                .WithOne(x => x.Group)
                .HasForeignKey(x => x.GroupId);

            builder.Ignore(x => x.Code);
        }
    }
}

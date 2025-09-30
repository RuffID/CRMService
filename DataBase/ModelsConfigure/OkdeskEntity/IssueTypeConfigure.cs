using CRMService.Models.OkdeskEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMService.DataBase.ModelsConfigure.OkdeskEntity
{
    public class IssueTypeConfigure : IEntityTypeConfiguration<IssueType>
    {
        public void Configure(EntityTypeBuilder<IssueType> builder)
        {
            builder.ToTable("issue_type");

            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

            builder.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");

            builder.Property(e => e.GroupId).HasColumnName("groupId");

            builder.HasIndex(e => e.GroupId, "groupId_idx");

            builder.Property(e => e.AvailableForClient).HasColumnName("available_for_client");

            builder.Property(e => e.Code).HasMaxLength(60).HasColumnName("code");

            builder.Property(e => e.IsDefault).HasColumnName("is_default");

            builder.Property(e => e.IsInner).HasColumnName("is_inner");

            builder.HasOne(x => x.Group)
                .WithMany(g => g.Types)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
